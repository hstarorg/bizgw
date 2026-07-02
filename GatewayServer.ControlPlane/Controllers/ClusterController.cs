using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Dtos;
using GatewayServer.ControlPlane.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.ControlPlane.Controllers
{
    /// <summary>集群 CRUD(读=任意登录,写=Owner/Editor)。</summary>
    [ApiController]
    [Route("api/clusters")]
    public sealed class ClusterController(ClusterService svc) : ControllerBase
    {
        [HttpGet("")]
        public async Task<object> List(int page = 1, int size = 20, string? keyword = null)
        {
            var (total, items) = await svc.ListAsync(page, size, keyword);
            return new { items, total, page, size };
        }

        [HttpGet("{id:long}")]
        public Task<ClusterDto> Get(long id) => svc.GetAsync(id);

        [Authorize(Roles = Roles.Writers)]
        [HttpPost("")]
        public Task<ClusterDto> Create(ClusterCreateRequest req) => svc.CreateAsync(req);

        [Authorize(Roles = Roles.Writers)]
        [HttpPut("{id:long}")]
        public async Task<object> Update(long id, ClusterUpdateRequest req)
        {
            await svc.UpdateAsync(id, req);
            return new { };
        }

        [Authorize(Roles = Roles.Writers)]
        [HttpDelete("{id:long}")]
        public async Task<object> Delete(long id)
        {
            await svc.DeleteAsync(id);
            return new { };
        }
    }
}
