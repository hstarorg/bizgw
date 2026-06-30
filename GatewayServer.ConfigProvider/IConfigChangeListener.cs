namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// 推轴·订阅侧:网关收到「配置变了」的信号。与拉轴正交,可注册多个、同时运行(叠加)。
    /// </summary>
    public interface IConfigChangeListener
    {
        /// <summary>
        /// 开始监听;观测到变更时回调 <paramref name="onChanged"/>(参数为观测到的版本,作廉价跳过用;
        /// 编排器仍以拉轴的版本为准)。直到 <paramref name="ct"/> 取消。
        /// </summary>
        Task StartAsync(Func<long, Task> onChanged, CancellationToken ct);
    }
}
