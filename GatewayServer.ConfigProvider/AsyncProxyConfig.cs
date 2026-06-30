using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.ConfigProvider
{
    public class AsyncProxyConfig : IProxyConfig
    {
        private readonly CancellationTokenSource cts = new();
        public AsyncProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            ChangeToken = new CancellationChangeToken(cts.Token);
            Routes = routes;
            Clusters = clusters;
        }
        public IReadOnlyList<RouteConfig> Routes { get; }

        public IReadOnlyList<ClusterConfig> Clusters { get; }

        public IChangeToken ChangeToken { get; }

        internal void SignalChange()
        {
            cts.Cancel();
        }
    }
}
