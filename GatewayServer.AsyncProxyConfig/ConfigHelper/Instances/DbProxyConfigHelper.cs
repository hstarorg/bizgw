using GatewayServer.AsyncProxyConfig.Entities;
using SqlSugar;

namespace GatewayServer.AsyncProxyConfig.ConfigHelper.Instances
{
    public class DbProxyConfigHelper : IAsyncProxyConfigHelper
    {
        private readonly SqlSugarScope db;
        public DbProxyConfigHelper()
        {
            db = new SqlSugarScope(new ConnectionConfig()
            {
                // server=192.168.31.250;port=3306;uid=root;pwd=localDev;database=gatewaydb
                ConnectionString = Environment.GetEnvironmentVariable("ConnectionString"), // 连接符字串
                DbType = DbType.MySql,//数据库类型
                IsAutoCloseConnection = true //不设成true要手动close
            });
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql);//输出sql,查看执行sql
            };
        }
        public async Task<ProxyConfigEntity> GetConfig()
        {
            var clusters = await this.QueryClustersAsync();
            var routes = await this.QueryRoutesAsync();

            return new ProxyConfigEntity(routes, clusters);
        }

        private async Task<List<RouteEntity>> QueryRoutesAsync()
        {
            var routes = await db.Queryable<RouteEntity>().Where(x => x.IsDeleted == 0).ToListAsync();
            return routes;
        }

        private async Task<List<ClusterEntity>> QueryClustersAsync()
        {
            // 查集群
            var clusters = await db.Queryable<ClusterEntity>().Where(x => x.IsDeleted == 0).ToListAsync();
            // 查集群目标
            var clusterDestinations = await db.Queryable<ClusterDestinationEntity>().Where(x => x.IsDeleted == 0).ToListAsync();

            // 目标字典
            var destDict = clusterDestinations.GroupBy(x => x.ClusterCode).ToDictionary(x => x.Key, x => x.ToList());

            // 遍历集群，将目标服务器加入集群
            clusters.ForEach(cluster =>
            {
                var firstItem = destDict.FirstOrDefault(x => x.Key == cluster.ClusterCode);
                if (firstItem.Value != null)
                {
                    firstItem.Value.ForEach(item =>
                    {
                        cluster.ClusterDestinations.Add(item);
                    });
                }
            });

            return clusters;
        }
    }
}