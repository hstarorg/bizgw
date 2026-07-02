using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Dtos;
using GatewayServer.ControlPlane.Http;
using GatewayServer.Data;
using GatewayServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.ControlPlane.Services
{
    /// <summary>集群编辑。cluster_code 创建后不可改(稳定连接键);删除允许(引用完整性发布期兜底)。</summary>
    public sealed class ClusterService(IDbContextFactory<GatewayDbContext> dbf, ICurrentUser current)
    {
        public async Task<(int total, List<ClusterDto> items)> ListAsync(int page, int size, string? keyword)
        {
            page = page < 1 ? 1 : page;
            size = size < 1 ? 20 : Math.Min(size, 200);

            await using var db = await dbf.CreateDbContextAsync();
            var q = db.Clusters.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(x => x.ClusterCode.Contains(keyword) || x.ClusterName.Contains(keyword));

            var total = await q.CountAsync();
            var rows = await q.OrderByDescending(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync();

            var codes = rows.Select(r => r.ClusterCode).ToList();
            var routeCounts = await RouteCountsAsync(db, codes);
            var destCounts = await DestinationCountsAsync(db, codes);
            return (total, rows.Select(r => ToDto(r,
                routeCounts.GetValueOrDefault(r.ClusterCode),
                destCounts.GetValueOrDefault(r.ClusterCode))).ToList());
        }

        public async Task<ClusterDto> GetAsync(long id)
        {
            await using var db = await dbf.CreateDbContextAsync();
            var row = await db.Clusters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw ApiException.NotFound("集群不存在");
            var used = await db.Routes.CountAsync(x => x.ClusterCode == row.ClusterCode);
            var dests = await db.Destinations.CountAsync(x => x.ClusterCode == row.ClusterCode);
            return ToDto(row, used, dests);
        }

        public async Task<ClusterDto> CreateAsync(ClusterCreateRequest req)
        {
            await using var db = await dbf.CreateDbContextAsync();
            if (await db.Clusters.AnyAsync(x => x.ClusterCode == req.ClusterCode))
                throw ApiException.Conflict($"cluster_code 已存在:{req.ClusterCode}");

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var row = new ClusterEntity { ClusterCode = req.ClusterCode, CreatorName = By, CreateDate = now };
            Apply(row, req);
            row.ModifierName = By;
            row.ModifyDate = now;
            db.Clusters.Add(row);
            await db.SaveChangesAsync();
            return ToDto(row, 0, 0);
        }

        public async Task UpdateAsync(long id, ClusterUpdateRequest req)
        {
            await using var db = await dbf.CreateDbContextAsync();
            var row = await db.Clusters.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw ApiException.NotFound("集群不存在");
            Apply(row, req);
            row.ModifierName = By;
            row.ModifyDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            await using var db = await dbf.CreateDbContextAsync();
            var row = await db.Clusters.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw ApiException.NotFound("集群不存在");
            row.IsDeleted = 1;
            row.ModifierName = By;
            row.ModifyDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await db.SaveChangesAsync();
        }

        private string By => current.Name ?? "system";

        private static async Task<Dictionary<string, int>> RouteCountsAsync(GatewayDbContext db, IEnumerable<string> codes)
        {
            var set = codes.ToHashSet();
            return await db.Routes.Where(x => set.Contains(x.ClusterCode))
                .GroupBy(x => x.ClusterCode)
                .Select(g => new { Code = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Code, x => x.Count);
        }

        private static async Task<Dictionary<string, int>> DestinationCountsAsync(GatewayDbContext db, IEnumerable<string> codes)
        {
            var set = codes.ToHashSet();
            return await db.Destinations.Where(x => set.Contains(x.ClusterCode))
                .GroupBy(x => x.ClusterCode)
                .Select(g => new { Code = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Code, x => x.Count);
        }

        private static void Apply(ClusterEntity row, ClusterUpdateRequest req)
        {
            row.ClusterName = req.ClusterName;
            row.LoadBalancingPolicy = req.LoadBalancingPolicy;
            row.EnabledHelthCheck = (short)(req.EnabledHealthCheck ? 1 : 0);
            row.HelthCheckInterval = req.HealthCheckInterval;
            row.HelthCheckTimeout = req.HealthCheckTimeout;
            row.HelthCheckPolicy = req.HealthCheckPolicy;
            row.HelthCheckPath = req.HealthCheckPath;
            row.Remark = req.Remark;
        }

        private static ClusterDto ToDto(ClusterEntity c, int usedByRouteCount, int destinationCount) => new()
        {
            DestinationCount = destinationCount,
            Id = c.Id,
            ClusterCode = c.ClusterCode,
            ClusterName = c.ClusterName,
            LoadBalancingPolicy = c.LoadBalancingPolicy,
            EnabledHealthCheck = c.EnabledHelthCheck > 0,
            HealthCheckInterval = c.HelthCheckInterval,
            HealthCheckTimeout = c.HelthCheckTimeout,
            HealthCheckPolicy = c.HelthCheckPolicy,
            HealthCheckPath = c.HelthCheckPath,
            Remark = c.Remark,
            UsedByRouteCount = usedByRouteCount,
            CreateDate = c.CreateDate,
            ModifyDate = c.ModifyDate,
            CreatorName = c.CreatorName,
            ModifierName = c.ModifierName,
        };
    }
}
