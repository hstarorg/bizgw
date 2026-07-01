using System.Security.Claims;

namespace GatewayServer.ControlPlane.Auth
{
    /// <summary>当前登录用户(供 Service 写审计列、判角色)。包 IHttpContextAccessor。</summary>
    public interface ICurrentUser
    {
        string? Name { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
    }

    public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
    {
        private ClaimsPrincipal? User => accessor.HttpContext?.User;

        public string? Name => User?.Identity?.Name;
        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;
        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }
}
