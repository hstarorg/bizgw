using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// 拉轴的返回契约:数据面消费的运行时配置(纯 YARP 类型),不含任何后端(DB/Redis)细节。
    /// </summary>
    public sealed class ProxyConfigData(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        public IReadOnlyList<RouteConfig> Routes { get; } = routes;

        public IReadOnlyList<ClusterConfig> Clusters { get; } = clusters;
    }
}
