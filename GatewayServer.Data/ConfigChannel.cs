namespace GatewayServer.Data
{
    /// <summary>
    /// 配置变更通知的契约:订阅侧(网关 LISTEN)与发布侧(控制面 NOTIFY)共用。
    /// 负载为新的版本号(字符串)。放在无 YARP 依赖的 Data 里,两侧可引。
    /// </summary>
    public static class ConfigChannel
    {
        public const string Name = "proxy_reload";
    }
}
