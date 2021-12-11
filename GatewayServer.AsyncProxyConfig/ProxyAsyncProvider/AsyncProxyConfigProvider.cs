using GatewayServer.AsyncProxyConfig;
using GatewayServer.AsyncProxyConfig.ConfigHelper;
using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.AsyncProxyConfig.ProxyAsyncProvider
{
    public class AsyncProxyConfigProvider : IProxyConfigProvider
    {
        private volatile AsyncProxyConfig _config;
        private readonly IAsyncProxyConfigHelper proxyConfigProvider;

        public AsyncProxyConfigProvider(IAsyncProxyConfigHelper proxyConfigProvider)
        {
            this.proxyConfigProvider = proxyConfigProvider;
            _config = new AsyncProxyConfig(new List<RouteConfig>(), new List<ClusterConfig>());
        }
        public IProxyConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// 初始化代理配置
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InitProxyConfig()
        {
            return await this.Reload();
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="clusters"></param>
        /// <returns></returns>
        public async Task<bool> Reload()
        {
            return await this.LoadConfigFromDb();
        }

        private async Task<bool> LoadConfigFromDb()
        {

            // 1. 拉取配置数据
            var proxyConfig = await proxyConfigProvider.GetConfig();

            // 2. 启动更新
            var oldConfig = _config;
            // 加载新配置
            _config = new AsyncProxyConfig(proxyConfig.routes, proxyConfig.clusters);
            // 释放老配置
            oldConfig.SignalChange();
            return true;

        }
    }
}
