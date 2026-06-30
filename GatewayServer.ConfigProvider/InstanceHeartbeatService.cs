using GatewayServer.Data;
using GatewayServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// 心跳上报:每隔若干秒 upsert 本实例的 instance_status 行(版本、reload 成败、心跳时间)。
    /// 间隔 <c>InstanceStatus:HeartbeatSeconds</c>,默认 10s。失败只记录、不影响代理。
    /// </summary>
    public class InstanceHeartbeatService(
        IDbContextFactory<GatewayDbContext> dbContextFactory,
        AsyncProxyConfigProvider provider,
        InstanceIdentity identity,
        IConfiguration configuration) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var seconds = configuration.GetValue<int?>("InstanceStatus:HeartbeatSeconds") ?? 10;
            var interval = TimeSpan.FromSeconds(seconds <= 0 ? 10 : seconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpsertAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("心跳上报失败: {0}", ex.Message);
                }

                try { await Task.Delay(interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task UpsertAsync(CancellationToken ct)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(ct);
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var row = await db.InstanceStatuses.FirstOrDefaultAsync(x => x.InstanceId == identity.InstanceId, ct);
            if (row == null)
            {
                db.InstanceStatuses.Add(new InstanceStatusEntity
                {
                    InstanceId = identity.InstanceId,
                    Hostname = identity.Hostname,
                    StartedAt = identity.StartedAt,
                    AppliedConfigVersion = provider.AppliedVersion,
                    LastReloadAt = provider.LastReloadAt,
                    LastReloadOk = provider.LastReloadOk,
                    LastHeartbeatAt = now,
                });
            }
            else
            {
                row.Hostname = identity.Hostname;
                row.StartedAt = identity.StartedAt;
                row.AppliedConfigVersion = provider.AppliedVersion;
                row.LastReloadAt = provider.LastReloadAt;
                row.LastReloadOk = provider.LastReloadOk;
                row.LastHeartbeatAt = now;
            }
            await db.SaveChangesAsync(ct);
        }
    }
}
