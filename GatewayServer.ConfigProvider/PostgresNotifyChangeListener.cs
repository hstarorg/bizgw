using GatewayServer.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// PG 专属:自开一条长连接 <c>LISTEN proxy_reload</c>,收到通知即触发回调(求快)。
    /// 断线自动重连;期间漏掉的通知由 Polling 兜底。仅加速,正确性不依赖它。
    /// </summary>
    public class PostgresNotifyChangeListener(IConfiguration configuration) : IConfigChangeListener
    {
        public async Task StartAsync(Func<long, Task> onChanged, CancellationToken ct)
        {
            var connectionString = configuration["ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("PostgresNotifyChangeListener: 缺少 ConnectionString,跳过(由 Polling 兜底)");
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connectionString);
                    await conn.OpenAsync(ct);

                    long pending = 0;
                    bool hasPending = false;
                    conn.Notification += (_, e) =>
                    {
                        pending = long.TryParse(e.Payload, out var v) ? v : long.MaxValue;
                        hasPending = true;
                    };

                    await using (var cmd = new NpgsqlCommand($"LISTEN {ConfigChannel.Name}", conn))
                    {
                        await cmd.ExecuteNonQueryAsync(ct);
                    }

                    while (!ct.IsCancellationRequested)
                    {
                        // 阻塞直到收到通知(同步抛出 Notification 事件后返回)
                        await conn.WaitAsync(ct);
                        if (hasPending)
                        {
                            hasPending = false;
                            await onChanged(pending);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // 连接断开等:记录后重连;窗口期由 Polling 兜底
                    Console.WriteLine("LISTEN 连接异常,5s 后重连: {0}", ex.Message);
                    try { await Task.Delay(TimeSpan.FromSeconds(5), ct); }
                    catch (OperationCanceledException) { break; }
                }
            }
        }
    }
}
