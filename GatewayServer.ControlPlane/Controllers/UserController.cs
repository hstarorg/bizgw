using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Dtos;
using GatewayServer.ControlPlane.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.ControlPlane.Controllers
{
    /// <summary>用户管理:仅 Owner。</summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = Roles.Owner)]
    public sealed class UserController(UserService users, ICurrentUser current) : ControllerBase
    {
        private string By => current.Name ?? "system";

        [HttpGet("")]
        public async Task<object> List()
        {
            var list = await users.ListAsync();
            return list.Select(u => new
            {
                u.Id, u.Username, u.Role, u.CreateDate, u.ModifyDate, u.CreatorName, u.ModifierName,
            });
        }

        [HttpPost("")]
        public async Task<object> Create(CreateUserRequest req)
        {
            var u = await users.CreateAsync(req.Username, req.Password, req.Role, By);
            return new { u.Id, u.Username, u.Role };
        }

        [HttpPut("{id:long}")]
        public async Task<object> UpdateRole(long id, UpdateUserRequest req)
        {
            await users.UpdateRoleAsync(id, req.Role, By);
            return new { };
        }

        [HttpPost("{id:long}/reset-password")]
        public async Task<object> ResetPassword(long id, ResetPasswordRequest req)
        {
            await users.ResetPasswordAsync(id, req.Password, By);
            return new { };
        }

        [HttpDelete("{id:long}")]
        public async Task<object> Delete(long id)
        {
            await users.DeleteAsync(id, By);
            return new { };
        }
    }
}
