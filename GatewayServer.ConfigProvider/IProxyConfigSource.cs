namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// 拉轴:数据面从某个后端(DB 的 active 快照 / 未来 Redis 等)取配置。后端无关。
    /// </summary>
    public interface IProxyConfigSource
    {
        /// <summary>取当前生效配置并映射为 YARP 配置。</summary>
        Task<ProxyConfigData> GetConfigAsync();

        /// <summary>取当前生效版本号(廉价比对,无生效配置时为 0)。</summary>
        Task<long> GetVersionAsync();
    }
}
