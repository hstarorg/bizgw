using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Config;
using GatewayServer.ControlPlane.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.ControlPlane.Controllers
{
    /// <summary>
    /// 配置发布/回滚:把规范化编辑配置编译成版本化快照供数据面消费。
    /// </summary>
    [ApiController]
    [Route("api/config")]
    public class ConfigController(ConfigPublishService publishService) : ControllerBase
    {
        private readonly ConfigPublishService publishService = publishService;

        /// <summary>当前生效(active)版本。</summary>
        [HttpGet("version")]
        public async Task<object> GetActiveVersion()
        {
            var version = await publishService.GetActiveVersionAsync();
            return new { activeVersion = version };
        }

        /// <summary>发布当前配置为新的 active 快照。</summary>
        [Authorize(Roles = Roles.Writers)]
        [HttpPost("publish")]
        public async Task<object> Publish()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var result = await publishService.PublishAsync(User?.Identity?.Name ?? "anonymous", now);
            if (!result.Success)
            {
                throw ApiException.Unprocessable("配置校验未通过", new { errors = result.Errors });
            }
            return new { version = result.Version };
        }

        /// <summary>回滚到指定历史版本(以新版本号重新发布其内容)。</summary>
        [Authorize(Roles = Roles.Writers)]
        [HttpPost("rollback/{version:long}")]
        public async Task<object> Rollback(long version)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var result = await publishService.RollbackAsync(version, now);
            if (!result.Success)
            {
                throw ApiException.Unprocessable("回滚失败", new { errors = result.Errors });
            }
            return new { version = result.Version };
        }
    }
}
