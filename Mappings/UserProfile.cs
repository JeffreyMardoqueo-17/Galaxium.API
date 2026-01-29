using AutoMapper;
using Galaxium.API.DTOs.Users;
using Galaxium.API.Entities;

namespace Galaxium.Api.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // 
            // UserCreateRequest -> User
            // 
            CreateMap<UserCreateRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // 
            // User -> UserResponse
            // 
            CreateMap<User, UserResponse>();

            // 
            // Tuple -> AuthResponse 
            // 
            CreateMap<(string accessToken, string refreshToken), AuthResponse>()
                .ConstructUsing(src =>
                    new AuthResponse(
                        src.accessToken,
                        src.refreshToken
                    )
                );

            // 
            // RefreshToken -> RefreshTokenResponse
            // 
            CreateMap<RefreshToken, RefreshTokenResponse>();
        }
    }
}



// namespace Galaxium.API.DTOs
// {
//     // DTO para crear un usuario (request)
//     public record UserCreateRequest(
//         string FullName,
//         string Username,
//         string Password,
//         int RoleId
//     );

//     // DTO para respuesta de usuario sin datos sensibles
//     public record UserResponse(
//         int Id,
//         string FullName,
//         string Username,
//         int RoleId,
//         bool IsActive,
//         DateTime CreatedAt
//     );

//     // DTO para login (request)
//     public record UserLoginRequest(
//         string Username,
//         string Password
//     );

//     // DTO para respuesta de tokens al autenticarse
//     public record AuthResponse(
//         string AccessToken,
//         string RefreshToken
//     );

//     // DTO para RefreshToken (opcional para exponer detalles)
//     public record RefreshTokenResponse(
//         string Token,
//         DateTime ExpiresAt,
//         bool IsRevoked
//     );
// }
