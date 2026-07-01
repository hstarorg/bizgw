using System.ComponentModel.DataAnnotations;

namespace GatewayServer.ControlPlane.Dtos
{
    public sealed class CreateUserRequest
    {
        [Required, MinLength(3)] public string Username { get; set; } = "";
        [Required, MinLength(6)] public string Password { get; set; } = "";
        [Required] public string Role { get; set; } = "";
    }

    public sealed class UpdateUserRequest
    {
        [Required] public string Role { get; set; } = "";
    }

    public sealed class ResetPasswordRequest
    {
        [Required, MinLength(6)] public string Password { get; set; } = "";
    }

    public sealed class ChangePasswordRequest
    {
        [Required] public string OldPassword { get; set; } = "";
        [Required, MinLength(6)] public string NewPassword { get; set; } = "";
    }
}
