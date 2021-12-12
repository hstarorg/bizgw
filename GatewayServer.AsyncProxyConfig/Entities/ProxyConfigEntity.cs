using Newtonsoft.Json;
using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.AsyncProxyConfig.Entities
{
    public class ProxyConfigEntity
    {
        public ProxyConfigEntity(List<RouteEntity> routes, List<ClusterEntity> clusters)
        {
            this.routes = this.ProcessRoutes(routes);
            this.clusters = this.ProcessCluster(clusters);
        }
        public IReadOnlyList<RouteConfig> routes { get; }

        public IReadOnlyList<ClusterConfig> clusters { get; }

        private List<RouteConfig> ProcessRoutes(List<RouteEntity> routes)
        {
            var routeList = new List<RouteConfig>();
            routes.ForEach(r =>
            {
                var routeConfig = new RouteConfig
                {
                    ClusterId = r.ClusterCode,
                    RouteId = $"Route_{r.Id}",
                    Match = new RouteMatch()
                    {
                        Path = r.MatchPath,
                        Methods = r.MatchMethods.Split('|')
                    },
                    // 处理请求转换
                    Transforms = this.BuildTransforms(r.Transforms)
                };
                routeList.Add(routeConfig);
            });
            return routeList;
        }

        private IReadOnlyList<IReadOnlyDictionary<string, string>> BuildTransforms(string transformsStr)
        {
            var transforms = new List<IReadOnlyDictionary<string, string>>();
            // 先解析出数据
            try
            {
                var transformList = JsonConvert.DeserializeObject<List<TransformItem>>(transformsStr);
                transformList.ForEach(transform =>
                {
                    transforms.Add(transform.Value);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return transforms;
        }

        private List<ClusterConfig> ProcessCluster(List<ClusterEntity> clusters)
        {
            var clusterList = new List<ClusterConfig>();
            clusters.ForEach(c =>
            {
                var destinations = c.ClusterDestinations
                .Select(x => new KeyValuePair<string, DestinationConfig>($"ClusterDest_{x.Id}", new DestinationConfig { Address = x.Address, Health = x.HelthCheckPath == "" ? null : x.HelthCheckPath }));
                var clusterConfig = new ClusterConfig()
                {
                    ClusterId = c.ClusterCode,
                    LoadBalancingPolicy = c.LoadBalancingPolicy,
                    HealthCheck = new HealthCheckConfig()
                    {
                        Active = new ActiveHealthCheckConfig
                        {
                            Enabled = c.EnabledHelthCheck > 0,
                            Interval = this.Second2TimeSpan(c.HelthCheckInterval),
                            Timeout = this.Second2TimeSpan(c.HelthCheckTimeout),
                            // 为空就不要赋值 path 了
                            Path = c.HelthCheckPath == "" ? null : c.HelthCheckPath,
                            Policy = c.HelthCheckPolicy,
                        }
                    },
                    Destinations = destinations.ToDictionary(x => x.Key, x => x.Value)
                };

                clusterList.Add(clusterConfig);
            });
            return clusterList;
        }

        private TimeSpan Second2TimeSpan(int second)
        {
            return TimeSpan.FromSeconds(second);
        }
    }
}
