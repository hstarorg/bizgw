# Multi-stage build for the management API. Build context MUST be the solution root:
#   docker build -f GatewayServer.ConfigrationAPI/Dockerfile -t bizgw/config-api:latest .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (copy only csproj files so this layer is cached unless deps change)
COPY ["GatewayServer.ConfigrationAPI/GatewayServer.ConfigrationAPI.csproj", "GatewayServer.ConfigrationAPI/"]
RUN dotnet restore "GatewayServer.ConfigrationAPI/GatewayServer.ConfigrationAPI.csproj"

# Build + publish
COPY . .
RUN dotnet publish "GatewayServer.ConfigrationAPI/GatewayServer.ConfigrationAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# The aspnet:10.0 image listens on 8080 by default and ships a non-root "app" user
EXPOSE 8080
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "GatewayServer.ConfigrationAPI.dll"]
