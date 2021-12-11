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
            // 简单工厂注册异步配置获取方式
            IAsyncProxyConfigHelper helperInstance;
            switch (helperType)
            {
                case AsyncConfigHelperType.DB:
                    helperInstance = new DbProxyConfigHelper();
                    break;
                default:
                    helperInstance = new DbProxyConfigHelper();
                    break;
            }

            builder.Services.AddSingleton<IAsyncProxyConfigHelper>(helperInstance!);
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
