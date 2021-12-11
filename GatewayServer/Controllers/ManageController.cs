using GatewayServer.AsyncProxyConfig.ProxyAsyncProvider;
using Microsoft.AspNetCore.Mvc;
using Yarp.ReverseProxy.Configuration;

namespace GatewayServer.Controllers
{
    [Route("", Order = -1)]
    [ApiController]
    public class ManageController : ControllerBase
    {
        private readonly IProxyConfigProvider proxyConfigProvider;
        public ManageController(IProxyConfigProvider proxyConfigProvider)
        {
            this.proxyConfigProvider = proxyConfigProvider;
        }

        [HttpPost("/reload")]
        public async Task<object> Reload()
        {
            // 1. 先校验 TOKEN

            // 2. 判空
            var dcp = proxyConfigProvider as AsyncProxyConfigProvider;

            if (dcp == null)
            {
                return null;
            }

            // 3. 实际执行 Reload
            await dcp.Reload();

            return new { A = "a" };
        }
    }
}
