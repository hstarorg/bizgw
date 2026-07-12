# Multi-stage build for the control plane (management API + web-ui). Build context MUST be the solution root:
#   docker build -f GatewayServer.ControlPlane/Dockerfile -t bizgw/control-plane:latest .

# web-ui SPA build (served by the control plane from /app/wwwroot)
FROM node:22-alpine AS ui
WORKDIR /ui
RUN corepack enable
# Install deps first (cached unless the lockfile changes)
COPY ["web-ui/package.json", "web-ui/pnpm-lock.yaml", "web-ui/pnpm-workspace.yaml", "./"]
RUN pnpm install --frozen-lockfile
COPY web-ui/ .
RUN pnpm build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (copy only csproj files so this layer is cached unless deps change)
COPY ["GatewayServer.ControlPlane/GatewayServer.ControlPlane.csproj", "GatewayServer.ControlPlane/"]
COPY ["GatewayServer.Data/GatewayServer.Data.csproj", "GatewayServer.Data/"]
RUN dotnet restore "GatewayServer.ControlPlane/GatewayServer.ControlPlane.csproj"

# Build + publish
COPY . .
RUN dotnet publish "GatewayServer.ControlPlane/GatewayServer.ControlPlane.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# The aspnet:10.0 image listens on 8080 by default and ships a non-root "app" user
EXPOSE 8080
COPY --from=build /app/publish .
COPY --from=ui /ui/dist ./wwwroot
USER app
ENTRYPOINT ["dotnet", "GatewayServer.ControlPlane.dll"]
