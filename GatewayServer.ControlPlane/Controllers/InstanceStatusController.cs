using GatewayServer.ControlPlane.Config;
using GatewayServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.ControlPlane.Controllers
{
    /// <summary>
    /// 集群视图:列出各网关实例的存活/版本/是否落后/上次 reload 成败。
    /// </summary>
    [ApiController]
    [Route("api/instances")]
    public class InstanceStatusController(
        IDbContextFactory<GatewayDbContext> dbContextFactory,
        ConfigPublishService publishService,
        IConfiguration configuration) : ControllerBase
    {
        [HttpGet("")]
        public async Task<object> List()
        {
            var offlineAfterSeconds = configuration.GetValue<int?>("InstanceStatus:OfflineAfterSeconds") ?? 45;
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var activeVersion = await publishService.GetActiveVersionAsync();

            await using var db = await dbContextFactory.CreateDbContextAsync();
            var rows = await db.InstanceStatuses.AsNoTracking().ToListAsync();

            return rows
                .Select(r => new
                {
                    r.InstanceId,
                    r.Hostname,
                    AppliedVersion = r.AppliedConfigVersion,
                    ActiveVersion = activeVersion,
                    Lagging = r.AppliedConfigVersion < activeVersion,
                    Online = (now - r.LastHeartbeatAt) <= offlineAfterSeconds * 1000L,
                    r.LastReloadOk,
                    r.LastReloadAt,
                    r.LastHeartbeatAt,
                    r.StartedAt,
                })
                .OrderByDescending(x => x.Online)
                .ThenBy(x => x.InstanceId)
                .ToList();
        }
    }
}
