using GatewayServer.ControlPlane.Config;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.ControlPlane.Controllers
{
    /// <summary>
    /// 配置发布/回滚:把规范化编辑配置编译成版本化快照供数据面消费。
    /// </summary>
    [ApiController]
    [Route("api/config")]
    public class ConfigController : ControllerBase
    {
        private readonly ConfigPublishService publishService;

        public ConfigController(ConfigPublishService publishService)
        {
            this.publishService = publishService;
        }

        /// <summary>当前生效(active)版本。</summary>
        [HttpGet("version")]
        public async Task<object> GetActiveVersion()
        {
            var version = await publishService.GetActiveVersionAsync();
            return new { ActiveVersion = version };
        }

        /// <summary>发布当前配置为新的 active 快照。</summary>
        [HttpPost("publish")]
        public async Task<IActionResult> Publish()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var result = await publishService.PublishAsync(User?.Identity?.Name ?? "anonymous", now);
            if (!result.Success)
            {
                return UnprocessableEntity(new { result.Success, result.Errors });
            }
            return Ok(new { result.Success, result.Version });
        }

        /// <summary>回滚到指定历史版本(以新版本号重新发布其内容)。</summary>
        [HttpPost("rollback/{version:long}")]
        public async Task<IActionResult> Rollback(long version)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var result = await publishService.RollbackAsync(version, now);
            if (!result.Success)
            {
                return UnprocessableEntity(new { result.Success, result.Errors });
            }
            return Ok(new { result.Success, result.Version });
        }
    }
}
