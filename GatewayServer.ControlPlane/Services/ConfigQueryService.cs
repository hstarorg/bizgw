using GatewayServer.Data;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.ControlPlane.Services
{
    /// <summary>配置只读查询:快照历史 + 草稿态。</summary>
    public sealed class ConfigQueryService(IDbContextFactory<GatewayDbContext> dbf)
    {
        public async Task<List<object>> ListSnapshotsAsync()
        {
            await using var db = await dbf.CreateDbContextAsync();
            return await db.ConfigSnapshots.AsNoTracking()
                .OrderByDescending(x => x.Version)
                .Select(x => (object)new
                {
                    x.Version,
                    x.IsActive,
                    x.SchemaVersion,
                    x.PublishedAt,
                    x.PublishedBy,
                })
                .ToListAsync();
        }

        /// <summary>
        /// 是否有未发布草稿:编辑表(含逻辑删除行,故忽略查询过滤)最大 modify_date &gt; active 快照 published_at。
        /// </summary>
        public async Task<bool> HasPendingChangesAsync()
        {
            await using var db = await dbf.CreateDbContextAsync();

            var activePublishedAt = await db.ConfigSnapshots
                .Where(x => x.IsActive).Select(x => (long?)x.PublishedAt).FirstOrDefaultAsync() ?? 0;

            var mr = await db.Routes.IgnoreQueryFilters().Select(x => (long?)x.ModifyDate).MaxAsync() ?? 0;
            var mc = await db.Clusters.IgnoreQueryFilters().Select(x => (long?)x.ModifyDate).MaxAsync() ?? 0;
            var md = await db.Destinations.IgnoreQueryFilters().Select(x => (long?)x.ModifyDate).MaxAsync() ?? 0;
            var maxEdit = Math.Max(mr, Math.Max(mc, md));

            return maxEdit > activePublishedAt;
        }
    }
}
