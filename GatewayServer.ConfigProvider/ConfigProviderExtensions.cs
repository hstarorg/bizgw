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
            services.AddSingleton<IProxyConfigSource, DbConfigSource>();
            return services;
        }

        /// <summary>
        /// 把 AsyncProxyConfigProvider 作为 YARP 的 IProxyConfigProvider 注册;后台首次加载,失败回调。
        /// </summary>
        public static IReverseProxyBuilder LoadFromAsyncProvider(this IReverseProxyBuilder builder, Action<bool, Exception?> loadCallbackFn)
        {
            builder.Services.AddSingleton<IProxyConfigProvider>(sp =>
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

            return builder;
        }
    }
}
