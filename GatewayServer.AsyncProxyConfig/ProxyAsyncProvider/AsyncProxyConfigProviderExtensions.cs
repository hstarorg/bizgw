using GatewayServer.AsyncProxyConfig.ConfigHelper;
using GatewayServer.AsyncProxyConfig.ConfigHelper.Instances;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.AsyncProxyConfig.ProxyAsyncProvider
{
    public static class AsyncProxyConfigProviderExtensions
    {
        public static IReverseProxyBuilder LoadFromAsyncProvider(this IReverseProxyBuilder builder, AsyncConfigHelperType helperType, Action<bool, Exception?> loadCallbackFn)
        {
            // 简单工厂注册异步配置获取方式（实例由 DI 构造，以便注入 GatewayDbContext 工厂）
            switch (helperType)
            {
                case AsyncConfigHelperType.DB:
                default:
                    builder.Services.AddSingleton<IAsyncProxyConfigHelper, DbProxyConfigHelper>();
                    break;
            }

            builder.Services.AddSingleton<IProxyConfigProvider>(sp =>
            {
                var dbProxyConfigProvider = new AsyncProxyConfigProvider(sp.GetService<IAsyncProxyConfigHelper>()!);
                Task.Run(async () =>
                {
                    try
                    {
                        await dbProxyConfigProvider.InitProxyConfig();
                        loadCallbackFn(true, null);
                    }
                    catch (Exception ex)
                    {
                        loadCallbackFn(false, ex);
                    }
                });
                return dbProxyConfigProvider;
            });

            return builder;
        }
    }
}
