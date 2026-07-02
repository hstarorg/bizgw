using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Dtos;
using GatewayServer.ControlPlane.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.ControlPlane.Controllers
{
    /// <summary>集群目标 CRUD,嵌套在集群下(读=任意登录,写=Owner/Editor)。</summary>
    [ApiController]
    [Route("api/clusters/{clusterCode}/destinations")]
    public sealed class DestinationController(DestinationService svc) : ControllerBase
    {
        [HttpGet("")]
        public Task<List<DestinationDto>> List(string clusterCode) => svc.ListAsync(clusterCode);

        [Authorize(Roles = Roles.Writers)]
        [HttpPost("")]
        public Task<DestinationDto> Create(string clusterCode, DestinationUpsertRequest req)
            => svc.CreateAsync(clusterCode, req);

        [Authorize(Roles = Roles.Writers)]
        [HttpPut("{id:long}")]
        public async Task<object> Update(string clusterCode, long id, DestinationUpsertRequest req)
        {
            await svc.UpdateAsync(clusterCode, id, req);
            return new { };
        }

        [Authorize(Roles = Roles.Writers)]
        [HttpDelete("{id:long}")]
        public async Task<object> Delete(string clusterCode, long id)
        {
            await svc.DeleteAsync(clusterCode, id);
            return new { };
        }
    }
}
