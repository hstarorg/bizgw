# Multi-stage build for the control plane (management API). Build context MUST be the solution root:
#   docker build -f GatewayServer.ControlPlane/Dockerfile -t bizgw/control-plane:latest .

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
USER app
ENTRYPOINT ["dotnet", "GatewayServer.ControlPlane.dll"]
