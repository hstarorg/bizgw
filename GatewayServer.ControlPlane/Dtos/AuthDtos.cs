using System.ComponentModel.DataAnnotations;

namespace GatewayServer.ControlPlane.Dtos
{
    public sealed class LoginRequest
    {
        [Required] public string Username { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }

    public sealed class SetupRequest
    {
        [Required, MinLength(3)] public string Username { get; set; } = "";
        [Required, MinLength(6)] public string Password { get; set; } = "";
    }
}
