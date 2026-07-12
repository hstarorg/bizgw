# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`bizgw` is a reverse-proxy gateway built on [YARP](https://github.com/microsoft/reverse-proxy). Unlike stock YARP (which loads routes/clusters from `appsettings.json`), this gateway loads its proxy configuration **from a PostgreSQL database at runtime** (via EF Core): the control plane edits normalized tables and **publishes immutable versioned snapshots**; gateway instances read the active snapshot and hot-swap it without restarting, and **propagate changes across all instances** via a change-notification axis (Postgres `LISTEN/NOTIFY` + polling fallback).

Source comments and README are in Chinese.

## Solution layout (`Bizgw.sln`, .NET 10)

Split along the standard control plane / data plane model; the two sides intersect only through the shared schema in `GatewayServer.Data`.

Data plane (runs traffic, high-availability, scaled to many instances):
- **GatewayServer** — the deployable gateway entry point. Hosts the YARP reverse proxy + a `/reload` endpoint. References **only `GatewayServer.ConfigProvider`** — not `Data` directly (the DB coupling lives in the provider lib, so a future Redis backend wouldn't touch the host). YARP-only host.
- **GatewayServer.ConfigProvider** — the config-provisioning library (the former `GatewayServer.AsyncProxyConfig`, renamed and grown). Holds: the pull source (`IProxyConfigSource` / `DbConfigSource`), provider plumbing (`AsyncProxyConfigProvider`, `AsyncProxyConfig`, `LoadFromAsyncProvider`, `AddConfigProvider`), entity→YARP mapping (`ProxyConfigEntity` / `TransformItem`), and the push axis (`IConfigChangeListener`, `PollingChangeListener`, `PostgresNotifyChangeListener`, `ConfigSyncService`). References `GatewayServer.Data` + YARP + Npgsql.
- **GatewayServer.Tests** — MSTest unit tests for `GatewayServer`.

Control plane (edits config, lower-availability, can run single-instance):
- **GatewayServer.ControlPlane** — management REST API (renamed from the old `ConfigrationAPI`). Edits normalized config and **publishes versioned snapshots** (`ConfigController` + `ConfigPublishService` + `ConfigValidator`), emitting `pg_notify` on publish. References only `GatewayServer.Data` (no YARP). Entity CRUD (`RouteController`/`ClusterController`/BLL/DAL) is still partly stubbed.
- **web-ui** — UmiJS 3 + React 17 + Ant Design Pro admin UI (pairs with ControlPlane). Also early-stage.

Shared:
- **GatewayServer.Data** — entities (`Route`/`Cluster`/`ClusterDestination`/`EntityBase` + `ConfigSnapshot`) + `GatewayDbContext` + `AddGatewayData` DI extension + design-time factory + EF Core `Migrations/` + the `ConfigChannel` notify-channel constant (`proxy_reload`). References EF Core / Npgsql / EF Core Design. No YARP.

Architectural reason for splitting the gateway from the management API: different availability requirements and different scaling needs (see README).

## How config flows (the key architecture)

Editing and runtime config are **separated**: the data plane never reads the editing tables — only immutable, versioned **snapshots**. This gives atomic activation, validate-before-publish, rollback, and decoupling of the gateway from the editing schema.

**Edit (control plane).** `route` / `cluster` / `destination` tables are the editable source of truth (normalized; `is_deleted` logical-delete via a global `HasQueryFilter`). The ControlPlane API mutates these (CRUD partly stubbed).

**Publish (control plane).** `POST /api/config/publish` (`ConfigController` → `ConfigPublishService`): serialize the current normalized config into a `ConfigSnapshotDoc`, **validate** it (`ConfigValidator`: route→cluster referential integrity, non-empty match path, parseable `transforms`, unique/non-empty cluster codes), then write a new **`config_snapshot`** row in one transaction (`version = max+1`, `is_active = true`, previous active cleared first; a partial unique index enforces a single active row). Invalid config → 422, never reaches a gateway. `POST /api/config/rollback/{version}` re-publishes an old snapshot's doc as a new version. After commit it fires `SELECT pg_notify('proxy_reload', '<version>')` (inline — keeps ControlPlane YARP-free).

**Load (data plane, `GatewayServer.ConfigProvider`).**
1. `GatewayServer/Program.cs` calls `AddConfigProvider(Configuration)` (registers `IDbContextFactory<GatewayDbContext>` via `AddGatewayData`, the pull source `IProxyConfigSource → DbConfigSource`, the configured change listeners, and the `ConfigSyncService` orchestrator), then `AddReverseProxy().LoadFromAsyncProvider(callback)`.
2. `LoadFromAsyncProvider` registers `AsyncProxyConfigProvider` as the YARP `IProxyConfigProvider`; initial load runs on a background `Task`, and the callback kills the process if the first load fails.
3. `DbConfigSource` (pull axis) reads the **active `config_snapshot` row only** (never the editing tables), deserializes `ConfigSnapshotDoc`, and maps entities → YARP via `ProxyConfigEntity` (route ids `Route_{id}`, destinations `ClusterDest_{id}`, `match_methods` `|`-split, `transforms` JSON → `TransformItem`, health-check → `ActiveHealthCheckConfig`). Returns `ProxyConfigData` (YARP `RouteConfig`/`ClusterConfig`). `GetVersionAsync()` cheaply reads the active version.
4. `AsyncProxyConfigProvider` holds a `volatile` config, swaps it atomically on reload, records `AppliedVersion`, and calls `SignalChange()` so YARP picks up the new routes.

**Propagate (push axis — two independent interfaces, never fused with the pull axis).**
- `IConfigChangeListener` (subscribe side, gateway) is registered **additively** per the `Listeners` config: `PollingChangeListener` (backend-neutral; polls `GetVersionAsync`) and `PostgresNotifyChangeListener` (PG `LISTEN proxy_reload`, auto-reconnect).
- Publish side is the inline `pg_notify` in ControlPlane; the only shared contract is `GatewayServer.Data.ConfigChannel.Name`.
- `ConfigSyncService` (a `BackgroundService`) fans in all listeners; on any signal it reads the authoritative version from the source and, if `> AppliedVersion`, calls `provider.Reload()` (serialized/deduped via a `SemaphoreSlim`). Polling is the correctness baseline; NOTIFY only accelerates — if NOTIFY is unavailable, polling still converges.

**Reload escape hatch.** `POST /reload` (`ManageController`, validates `AuthCode`) forces a single-instance `Reload()` — kept for ops, no longer the fan-out mechanism.

**Health & status.** Each gateway exposes `/healthz` (liveness) and `/readyz` (readiness = DB reachable + first config loaded, via `ReadinessHealthCheck`), and a heartbeat service (`InstanceHeartbeatService` + `InstanceIdentity`) upserts an `instance_status` row (applied version, last reload ok, heartbeat time). The control plane's `GET /api/instances` reads those rows and derives per-instance `Online` (heartbeat freshness) and `Lagging` (`appliedVersion < activeVersion`).

**Observability (opt-in, gateway only).** `GatewayServer/Observability/ObservabilityExtensions.cs` (`AddObservability`) wires OpenTelemetry traces/metrics/logs over OTLP — but only when `Observability__Enabled=true`; off by default with zero registration. Inlined in the gateway host (no separate project); the control plane isn't instrumented. Metrics cover ASP.NET Core / Kestrel / HttpClient / runtime; YARP forwarding shows up via HttpClient instrumentation (`http.client.*`), since YARP 2.3.0 doesn't expose its own `Yarp.ReverseProxy` Meter (the `AddMeter` subscription is a forward-compatible no-op).

To add a new config backend (e.g. Redis): add `RedisConfigSource : IProxyConfigSource` (+ optional `RedisChangeListener : IConfigChangeListener`) in `GatewayServer.ConfigProvider` and wire it in `AddConfigProvider`; the gateway host stays untouched. A fully clean multi-backend split would extract an abstractions assembly — deferred until actually needed (project is small).

## Environment variables (both GatewayServer and ControlPlane)

- `ConnectionString` (**required**) — PostgreSQL (Npgsql) connection string, e.g. `Host=...;Port=5432;Username=postgres;Password=...;Database=gatewaydb`. Read by `AddGatewayData` (env via `IConfiguration`, env wins), the design-time factory, and the `PostgresNotify` listener (opens its own `LISTEN` connection).
- `AuthCode` (optional, GatewayServer) — required in the `/reload` request body. If unset, a random one is generated per start and printed to the startup log.
- `Listeners` (optional, GatewayServer) — comma-separated change listeners; default `Polling`. Recommended on PG: `PostgresNotify,Polling` (NOTIFY for speed, polling as the correctness net).
- `ConfigSync__PollIntervalSeconds` (optional, GatewayServer) — polling interval in seconds, default `15`.
- `INSTANCE_ID` (optional, GatewayServer) — instance id for status reporting; falls back to `HOSTNAME`, then a generated GUID. Set distinct per replica.
- `InstanceStatus__HeartbeatSeconds` (optional, GatewayServer) — heartbeat upsert interval, default `10`.
- `InstanceStatus__OfflineAfterSeconds` (optional, ControlPlane) — `/api/instances` marks an instance offline if its last heartbeat is older than this, default `45`.
- `Observability__Enabled` (optional, GatewayServer) — turns on OpenTelemetry (traces/metrics/logs via OTLP). **Default `false`** — when off, no OTel is registered (zero overhead, no collector needed). When on, the standard `OTEL_*` env vars apply (`OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_TRACES_SAMPLER`, `OTEL_SERVICE_NAME`).

## Database

PostgreSQL via EF Core. Schema is managed by **EF Core Migrations** in `GatewayServer.Data/Migrations/` — create/upgrade tables with `dotnet ef database update --project GatewayServer.Data` (needs `ConnectionString`). Entities use EF data annotations (`[Table]`/`[Column]`/`[Key]`/`[DatabaseGenerated]`) mapping PascalCase props to snake_case columns. The editing tables (`route`/`cluster`/`destination`) use logical delete (`is_deleted`, enforced via a global `HasQueryFilter`). The append-only **`config_snapshot`** table (`version` bigint PK, `doc` jsonb, `schema_version`, `is_active` with a partial unique index `WHERE is_active`, `published_at`/`published_by`) holds published runtime config; the data plane reads only its active row. The legacy MySQL DDL in `docs/*.sql` is deprecated — historical reference only.

## Commands

### .NET (run from repo root)
```bash
dotnet build Bizgw.sln
dotnet run --project GatewayServer                 # run the gateway (needs ConnectionString env)
dotnet run --project GatewayServer.ControlPlane    # run the management API (Swagger in Development)
dotnet test                                        # run all tests
dotnet test --filter TestGenerateRandomString      # run a single test

# EF Core migrations (run from repo root; needs ConnectionString for `database update`)
dotnet ef migrations add <Name> --project GatewayServer.Data
dotnet ef database update --project GatewayServer.Data
```

### web-ui (run from `web-ui/`, uses pnpm)
```bash
pnpm install
pnpm start        # umi dev server
pnpm build        # umi build
pnpm test         # umi-test
pnpm prettier     # format
```

## Deployment

Docker-based, .NET 10. Each .NET project's `Dockerfile` is a self-contained multi-stage build (restore → publish → run) — **the build context is the solution root**, e.g. `docker build -f GatewayServer/Dockerfile -t bizgw/gateway .`. Containers listen on **8080** (the .NET 8+ image default, non-root `app` user) — not the old port 80. `docker-compose.yml` brings up PostgreSQL 18 (`db`) + gateway (8889) + control-plane (8890). GatewayServer needs external network exposure; the API + UI should stay internal. See README for full steps.

The compose `db` service starts an empty database; migrations are not run automatically. After first start, create the tables once with `dotnet ef database update --project GatewayServer.Data` pointed at the db.

## Examples

`examples/server01` and `examples/server02` are tiny Node servers used as proxy backends for local testing.
