# web-ui

Bizgw 控制面管理后台。React 19 + Vite (Rolldown) + TypeScript + Tailwind v4 + shadcn/ui。

## 开发

```bash
pnpm install
pnpm dev        # http://localhost:5173，/api 代理到 ControlPlane (localhost:5160)
```

先启动 ControlPlane（`dotnet run --project GatewayServer.ControlPlane`，默认 5160），前端 dev server 会把 `/api` 请求代理过去。

## 构建

```bash
pnpm build      # 产物在 dist/（部署时由 ControlPlane Dockerfile 拷入 wwwroot）
pnpm preview
```

## 加 shadcn 组件

```bash
pnpm dlx shadcn@latest add button table dialog ...
```

组件落在 `src/components/ui`，别名 `@/` → `src/`。
