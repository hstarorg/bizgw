using GatewayServer.AsyncProxyConfig.ProxyAsyncProvider;
using GatewayServer.Dtos;
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
        public async Task<object> Reload(ReloadDTO reloadDto)
        {
            // 1. 先校验 TOKEN
            if (reloadDto?.AuthCode != GlobalConfig.AuthCode)
            {
                return new JsonResult(new
                {
                    Result = false,
                    ErrorMsg = "未授权访问"
                })
                { StatusCode = 401 };
            }

            // 2. 判空
            var dcp = proxyConfigProvider as AsyncProxyConfigProvider;

            if (dcp == null)
            {
                return new JsonResult(new
                {
                    Result = false,
                    ErrorMsg = "获取 AsyncProxyConfigProvider 异常"
                })
                {
                    StatusCode = 500
                };
            }

            // 3. 实际执行 Reload
            await dcp.Reload();

            // 一切正常，返回成功
            return new JsonResult(new
            {
                Result = true
            });
        }
    }
}
