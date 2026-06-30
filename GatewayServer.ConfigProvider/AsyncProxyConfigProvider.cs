using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.ConfigProvider
{
    public class AsyncProxyConfigProvider(IProxyConfigSource proxyConfigSource) : IProxyConfigProvider
    {
        private volatile AsyncProxyConfig _config = new([], []);
        private readonly IProxyConfigSource proxyConfigSource = proxyConfigSource;

        /// <summary>本实例当前已应用的配置版本(快照 version,初始 0)。</summary>
        public long AppliedVersion { get; private set; }

        /// <summary>是否已完成首次配置加载(供 readiness 探针判断)。</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>上次 reload 时间(epoch ms)与成败,供心跳上报。</summary>
        public long LastReloadAt { get; private set; }
        public bool LastReloadOk { get; private set; }

        public IProxyConfig GetConfig()
        {
            return _config;
        }

        /// <summary>初始化代理配置。</summary>
        public async Task<bool> InitProxyConfig()
        {
            return await this.Reload();
        }

        /// <summary>重新加载配置(从拉轴取 active 快照,原子替换)。</summary>
        public async Task<bool> Reload()
        {
            try
            {
                // 1. 拉取配置数据(active 快照)及其版本
                var version = await proxyConfigSource.GetVersionAsync();
                var data = await proxyConfigSource.GetConfigAsync();

                // 2. 原子替换
                var oldConfig = _config;
                _config = new AsyncProxyConfig(data.Routes, data.Clusters);
                AppliedVersion = version;
                IsInitialized = true;
                LastReloadOk = true;
                LastReloadAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                // 释放老配置,触发 YARP 重新读取
                oldConfig.SignalChange();
                return true;
            }
            catch
            {
                LastReloadOk = false;
                LastReloadAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                throw;
            }
        }
    }
}
