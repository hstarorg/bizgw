using GatewayServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// readiness:DB 可达 + 首次配置已加载。供 LB/K8s 决定是否给本实例派流量。
    /// </summary>
    public class ReadinessHealthCheck(
        IDbContextFactory<GatewayDbContext> dbContextFactory,
        AsyncProxyConfigProvider provider) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!provider.IsInitialized)
            {
                return HealthCheckResult.Unhealthy("首次配置尚未加载");
            }

            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                if (!await db.Database.CanConnectAsync(cancellationToken))
                {
                    return HealthCheckResult.Unhealthy("数据库不可达");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("数据库探测失败", ex);
            }

            return HealthCheckResult.Healthy($"就绪,已应用版本 {provider.AppliedVersion}");
        }
    }
}
