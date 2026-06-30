using GatewayServer.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.ConfigProvider
{
    public static class ConfigProviderExtensions
    {
        /// <summary>
        /// 注册配置数据访问 + 拉源(默认 DB)。host 调它即可,无需直接引用 Data。
        /// </summary>
        public static IServiceCollection AddConfigProvider(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGatewayData(configuration);

            // 拉轴(默认 DB)
            services.AddSingleton<IProxyConfigSource, DbConfigSource>();

            // 推轴·订阅侧:按配置 Listeners 注册(默认 Polling;PG 推荐 "PostgresNotify,Polling")
            var listeners = (configuration["Listeners"] ?? "Polling")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var name in listeners)
            {
                switch (name.ToLowerInvariant())
                {
                    case "polling":
                        services.AddSingleton<IConfigChangeListener, PollingChangeListener>();
                        break;
                    case "postgresnotify":
                        services.AddSingleton<IConfigChangeListener, PostgresNotifyChangeListener>();
                        break;
                    default:
                        Console.WriteLine("未知的 Listeners 配置项,忽略: {0}", name);
                        break;
                }
            }

            // 编排器:扇入 listener → 比对版本 → Reload
            services.AddHostedService<ConfigSyncService>();
            return services;
        }

        /// <summary>
        /// 把 AsyncProxyConfigProvider 作为 YARP 的 IProxyConfigProvider 注册;后台首次加载,失败回调。
        /// </summary>
        public static IReverseProxyBuilder LoadFromAsyncProvider(this IReverseProxyBuilder builder, Action<bool, Exception?> loadCallbackFn)
        {
            // 注册具体 provider(编排器要用它读 AppliedVersion + Reload),并暴露为 YARP 的 IProxyConfigProvider
            builder.Services.AddSingleton<AsyncProxyConfigProvider>(sp =>
            {
                var provider = new AsyncProxyConfigProvider(sp.GetRequiredService<IProxyConfigSource>());
                Task.Run(async () =>
                {
                    try
                    {
                        await provider.InitProxyConfig();
                        loadCallbackFn(true, null);
                    }
                    catch (Exception ex)
                    {
                        loadCallbackFn(false, ex);
                    }
                });
                return provider;
            });
            builder.Services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<AsyncProxyConfigProvider>());

            return builder;
        }
    }
}
