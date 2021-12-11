using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.ProxyConfigProviders.DbProxyConfigProvider
{
    public static class DbProxyConfigProviderExtensions
    {
        public static IReverseProxyBuilder LoadFromDb(this IReverseProxyBuilder builder, IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            builder.Services.AddSingleton<IProxyConfigProvider>(new DbProxyConfigProvider(routes, clusters));
            return builder;
        }
    }
}
