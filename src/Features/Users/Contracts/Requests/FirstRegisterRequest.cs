namespace Galaxium.API.DTOs.Users
{
    public record FirstRegisterRequest(
        string TenantName,
        string FullName,
        string Username,
        string Password
    );
}