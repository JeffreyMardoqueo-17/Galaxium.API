namespace Galaxium.API.DTOs.Users
{
    public record ResetPasswordRequest(string Email, string Code, string NewPassword);
}
