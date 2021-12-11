﻿using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.ProxyConfigProviders.DbProxyConfigProvider
{
    public class DbProxyConfig : IProxyConfig
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        public DbProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
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
