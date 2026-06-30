using GatewayServer.AsyncProxyConfig;
using GatewayServer.AsyncProxyConfig.ConfigHelper;
using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.AsyncProxyConfig.ProxyAsyncProvider
{
    public class AsyncProxyConfigProvider : IProxyConfigProvider
    {
        private volatile AsyncProxyConfig _config;
        private readonly IProxyConfigSource proxyConfigSource;

        /// <summary>本实例当前已应用的配置版本(快照 version,初始 0)。</summary>
        public long AppliedVersion { get; private set; }

        public AsyncProxyConfigProvider(IProxyConfigSource proxyConfigSource)
        {
            this.proxyConfigSource = proxyConfigSource;
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

            // 1. 拉取配置数据(active 快照)及其版本
            var version = await proxyConfigSource.GetVersionAsync();
            var proxyConfig = await proxyConfigSource.GetConfigAsync();

            // 2. 原子替换
            var oldConfig = _config;
            // 加载新配置
            _config = new AsyncProxyConfig(proxyConfig.routes, proxyConfig.clusters);
            AppliedVersion = version;
            // 释放老配置
            oldConfig.SignalChange();
            return true;

        }
    }
}
