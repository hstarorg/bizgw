using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Dtos;
using GatewayServer.ControlPlane.Http;
using GatewayServer.ControlPlane.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayServer.ControlPlane.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController(UserService users, ICurrentUser current) : ControllerBase
    {
        /// <summary>启动态:是否需初始化 + 是否已登录。</summary>
        [AllowAnonymous]
        [HttpGet("status")]
        public async Task<object> Status()
        {
            var needsSetup = !await users.AnyUserAsync();
            return new
            {
                needsSetup,
                authenticated = current.IsAuthenticated,
                user = current.IsAuthenticated ? new { username = current.Name, role = current.Role } : null,
            };
        }

        /// <summary>首次初始化:创建首位 Owner 并登录。仅当用户表为空。</summary>
        [AllowAnonymous]
        [HttpPost("setup")]
        public async Task<object> Setup(SetupRequest req)
        {
            var user = await users.SetupOwnerAsync(req.Username, req.Password);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, UserService.BuildPrincipal(user));
            return new { username = user.Username, role = user.Role };
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<object> Login(LoginRequest req)
        {
            var user = await users.FindByUsernameAsync(req.Username);
            if (user == null || !users.VerifyPassword(user, req.Password))
            {
                throw new ApiException(401, "用户名或密码错误");
            }
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, UserService.BuildPrincipal(user));
            return new { username = user.Username, role = user.Role };
        }

        [HttpPost("logout")]
        public async Task<object> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return new { };
        }

        [HttpGet("me")]
        public object Me() => new { username = current.Name, role = current.Role };
    }
}
