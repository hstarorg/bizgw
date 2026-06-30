using Microsoft.Extensions.Hosting;

namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// 编排器:启动所有 listener(扇入),任一报变更 → 以拉轴版本为准比对 → 落后才 Reload。
    /// 不关心 listener / source 的具体实现。Reload 串行 + 去重。
    /// </summary>
    public class ConfigSyncService(
        IEnumerable<IConfigChangeListener> listeners,
        IProxyConfigSource source,
        AsyncProxyConfigProvider provider) : BackgroundService
    {
        private readonly SemaphoreSlim gate = new(1, 1);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = listeners.Select(l => l.StartAsync(OnChangedAsync, stoppingToken));
            return Task.WhenAll(tasks);
        }

        private async Task OnChangedAsync(long observedVersion)
        {
            // 廉价跳过:观测版本不超过已应用版本,无需动
            if (observedVersion <= provider.AppliedVersion)
            {
                return;
            }

            await gate.WaitAsync();
            try
            {
                // 以拉轴为权威:再确认一次当前生效版本,确实更新才 Reload
                var current = await source.GetVersionAsync();
                if (current > provider.AppliedVersion)
                {
                    await provider.Reload();
                    Console.WriteLine("配置已同步到版本 {0}", provider.AppliedVersion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("配置同步失败: {0}", ex.Message);
            }
            finally
            {
                gate.Release();
            }
        }
    }
}
