using GatewayServer.AsyncProxyConfig.Data;
using GatewayServer.AsyncProxyConfig.Entities;
using Microsoft.EntityFrameworkCore;

namespace GatewayServer.AsyncProxyConfig.ConfigHelper.Instances
{
    public class DbProxyConfigHelper : IAsyncProxyConfigHelper
    {
        private readonly IDbContextFactory<GatewayDbContext> dbContextFactory;

        public DbProxyConfigHelper(IDbContextFactory<GatewayDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task<ProxyConfigEntity> GetConfig()
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();

            // 逻辑删除由 GatewayDbContext 的全局查询过滤器统一处理
            var routes = await db.Routes.AsNoTracking().ToListAsync();
            var clusters = await db.Clusters.AsNoTracking().ToListAsync();
            var destinations = await db.Destinations.AsNoTracking().ToListAsync();

            // 目标按 cluster_code 分组，填入各集群
            var destDict = destinations.GroupBy(x => x.ClusterCode).ToDictionary(x => x.Key, x => x.ToList());
            clusters.ForEach(cluster =>
            {
                if (destDict.TryGetValue(cluster.ClusterCode, out var items))
                {
                    items.ForEach(item => cluster.ClusterDestinations.Add(item));
                }
            });

            return new ProxyConfigEntity(routes, clusters);
        }
    }
}
