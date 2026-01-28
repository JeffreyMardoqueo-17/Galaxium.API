using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace Galaxium.API.Utils
{
    public static class PasswordHasher
    {
        /// <summary>
        /// Genera un hash SHA256 usando la contraseña y un salt.
        /// </summary>
        public static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = password + salt;
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash); // retorna como string hexadecimal
        }

        /// <summary>
        /// Valida que una contraseña coincida con un hash y salt almacenados
        /// </summary>
        public static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            var hash = HashPassword(enteredPassword, storedSalt);
            return string.Equals(hash, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Genera un salt aleatorio en hexadecimal
        /// </summary>
        public static string GenerateSalt(int length = 32)
        {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes);
        }
    }
}