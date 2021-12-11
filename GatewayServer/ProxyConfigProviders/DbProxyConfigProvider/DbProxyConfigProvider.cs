using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.ProxyConfigProviders.DbProxyConfigProvider
{
    public class DbProxyConfigProvider : IProxyConfigProvider
    {
        private volatile DbProxyConfig _config;

        public DbProxyConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            _config = new DbProxyConfig(routes, clusters);
        }
        public IProxyConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="clusters"></param>
        /// <returns></returns>
        public async Task<bool> Reload()
        {
            var oldConfig = _config;
            // 加载新配置
            //_config = new DbProxyConfig(routes, clusters);
            // 释放老配置
            oldConfig.SignalChange();
            return await Task.Run(() => true);
        }

    }
}
