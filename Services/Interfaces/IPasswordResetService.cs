using System.Threading.Tasks;

namespace Galaxium.Api.Services.Interfaces
{
    public interface IPasswordResetService
    {
        /// <summary>
        /// Genera un código de 6 dígitos, lo guarda en BD y lo envía por correo.
        /// Siempre devuelve true para no revelar si el email existe.
        /// </summary>
        Task<bool> SendResetCodeAsync(string email);

        /// <summary>
        /// Valida el código y, si es correcto, actualiza la contraseña del usuario.
        /// Devuelve false si el código es inválido o expiró.
        /// </summary>
        Task<bool> ResetPasswordAsync(string email, string code, string newPassword);
    }
}
