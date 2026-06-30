using Microsoft.Extensions.Configuration;

namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// 通用基线:定时拉版本,变了就通知。只依赖拉轴,后端无关;作为正确性兜底(NOTIFY 漏了也能补上)。
    /// 间隔取配置 <c>ConfigSync:PollIntervalSeconds</c>,默认 15s。
    /// </summary>
    public class PollingChangeListener(IProxyConfigSource source, IConfiguration configuration) : IConfigChangeListener
    {
        public async Task StartAsync(Func<long, Task> onChanged, CancellationToken ct)
        {
            var seconds = configuration.GetValue<int?>("ConfigSync:PollIntervalSeconds") ?? 15;
            var interval = TimeSpan.FromSeconds(seconds <= 0 ? 15 : seconds);

            long lastSeen = 0;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var version = await source.GetVersionAsync();
                    if (version != lastSeen)
                    {
                        lastSeen = version;
                        await onChanged(version);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("轮询拉取版本失败: {0}", ex.Message);
                }

                try { await Task.Delay(interval, ct); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
