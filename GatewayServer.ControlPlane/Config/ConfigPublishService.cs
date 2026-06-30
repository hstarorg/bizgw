using System.Text.Json;
using GatewayServer.Data;
using GatewayServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.ControlPlane.Config
{
    /// <summary>
    /// 发布流程:序列化当前规范化配置 → 校验 → 通过才写新快照行 + 切 active + version++。
    /// 追加式;失败不落地。回滚 = 把旧快照 doc 重发为新版本。
    /// </summary>
    public class ConfigPublishService
    {
        private readonly IDbContextFactory<GatewayDbContext> dbContextFactory;

        public ConfigPublishService(IDbContextFactory<GatewayDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public record PublishResult(bool Success, long Version, IReadOnlyList<string> Errors);

        /// <summary>当前生效版本(无则 0)。</summary>
        public async Task<long> GetActiveVersionAsync()
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            return await db.ConfigSnapshots.AsNoTracking()
                .Where(x => x.IsActive).Select(x => x.Version).FirstOrDefaultAsync();
        }

        /// <summary>把当前规范化配置编译、校验并发布为新的 active 快照。</summary>
        public async Task<PublishResult> PublishAsync(string publishedBy, long nowEpochMs)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();

            var doc = new ConfigSnapshotDoc
            {
                Routes = await db.Routes.AsNoTracking().ToListAsync(),
                Clusters = await db.Clusters.AsNoTracking().ToListAsync(),
                Destinations = await db.Destinations.AsNoTracking().ToListAsync(),
            };

            var errors = ConfigValidator.Validate(doc);
            if (errors.Count > 0)
            {
                return new PublishResult(false, 0, errors);
            }

            var json = JsonSerializer.Serialize(doc);
            var version = await WriteActiveSnapshotAsync(db, json, ConfigSnapshotDoc.CurrentSchemaVersion, publishedBy, nowEpochMs);
            return new PublishResult(true, version, []);
        }

        /// <summary>回滚:取目标版本的 doc,作为新版本重新发布(版本号保持单调)。</summary>
        public async Task<PublishResult> RollbackAsync(long targetVersion, long nowEpochMs)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();

            var target = await db.ConfigSnapshots.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Version == targetVersion);
            if (target == null)
            {
                return new PublishResult(false, 0, [$"目标版本 {targetVersion} 不存在。"]);
            }

            var version = await WriteActiveSnapshotAsync(db, target.Doc, target.SchemaVersion, $"rollback:{targetVersion}", nowEpochMs);
            return new PublishResult(true, version, []);
        }

        /// <summary>事务内:停用旧 active 行 → 追加新 active 行(version = max + 1)。</summary>
        private static async Task<long> WriteActiveSnapshotAsync(
            GatewayDbContext db, string doc, int schemaVersion, string publishedBy, long nowEpochMs)
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            // 先停用旧 active(部分唯一索引要求 active 行唯一,需先清后插)
            await db.ConfigSnapshots.Where(x => x.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

            var maxVersion = await db.ConfigSnapshots.MaxAsync(x => (long?)x.Version) ?? 0;
            var version = maxVersion + 1;

            db.ConfigSnapshots.Add(new ConfigSnapshotEntity
            {
                Version = version,
                Doc = doc,
                SchemaVersion = schemaVersion,
                IsActive = true,
                PublishedAt = nowEpochMs,
                PublishedBy = publishedBy,
            });
            await db.SaveChangesAsync();

            await tx.CommitAsync();

            // 发布侧(内联,保 ControlPlane 无 YARP):提交后通知数据面,订阅侧 LISTEN 收到即比对 reload。
            // 仅加速;即使没收到,网关 Polling 也会兜底追平。
            await db.Database.ExecuteSqlRawAsync("SELECT pg_notify({0}, {1})", ConfigChannel.Name, version.ToString());

            return version;
        }
    }
}
