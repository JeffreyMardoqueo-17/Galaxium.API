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
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public static bool VerifyPassword(string enteredPassword, string storedHash)
    {
        var hash = HashPassword(enteredPassword);
        return string.Equals(hash, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}

}