using System.Text.Json;
using GatewayServer.Data;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// 默认拉轴实现:读 config_snapshot 的 active 行。数据面不直接查 route/cluster/destination 编辑表。
    /// </summary>
    public class DbConfigSource(IDbContextFactory<GatewayDbContext> dbContextFactory) : IProxyConfigSource
    {
        private readonly IDbContextFactory<GatewayDbContext> dbContextFactory = dbContextFactory;

        public async Task<long> GetVersionAsync()
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            return await db.ConfigSnapshots
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => x.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<ProxyConfigData> GetConfigAsync()
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();

            var snapshot = await db.ConfigSnapshots
                .AsNoTracking()
                .Where(x => x.IsActive)
                .FirstOrDefaultAsync();

            // 尚无任何发布版本:返回空配置(网关空跑,等待首次发布)
            if (snapshot == null)
            {
                return new ProxyConfigData([], []);
            }

            var doc = JsonSerializer.Deserialize<ConfigSnapshotDoc>(snapshot.Doc)
                ?? throw new InvalidOperationException($"配置快照 version={snapshot.Version} 反序列化为空。");

            // 目标按 cluster_code 分组,填入各集群
            var destDict = doc.Destinations.GroupBy(x => x.ClusterCode).ToDictionary(x => x.Key, x => x.ToList());
            doc.Clusters.ForEach(cluster =>
            {
                if (destDict.TryGetValue(cluster.ClusterCode, out var items))
                {
                    items.ForEach(item => cluster.ClusterDestinations.Add(item));
                }
            });

            // 实体 → YARP 映射
            var mapped = new ProxyConfigEntity(doc.Routes, doc.Clusters);
            return new ProxyConfigData(mapped.routes, mapped.clusters);
        }
    }
}
