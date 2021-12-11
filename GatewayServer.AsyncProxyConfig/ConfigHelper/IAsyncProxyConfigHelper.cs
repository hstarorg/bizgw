using GatewayServer.AsyncProxyConfig.Entities;

namespace GatewayServer.AsyncProxyConfig.ConfigHelper
{
    public interface IAsyncProxyConfigHelper
    {
        Task<ProxyConfigEntity> GetConfig();
    }
}