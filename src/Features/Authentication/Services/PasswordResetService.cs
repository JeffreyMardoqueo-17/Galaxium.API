using System;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Entities;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Data;
using Galaxium.API.Utils;
using Galaxium.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Galaxium.Api.Services.Service
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly GalaxiumDbContext _context;
        private readonly IEmailService _emailService;

        // Tiempo de vida del código (15 minutos)
        private const int CODE_TTL_MINUTES = 15;
        // Anti-spam: mínimo de segundos entre solicitudes para el mismo usuario
        private const int MIN_RESEND_SECONDS = 60;

        public PasswordResetService(GalaxiumDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> SendResetCodeAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            // El Username es el correo electrónico del usuario
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedEmail);

            // Siempre retornar true para no revelar si el usuario existe (anti-enumeración)
            if (user == null)
                return true;

            // Anti-spam: no enviar si ya se creó un código en el último minuto
            var recentCode = await _context.PasswordResetCode
                .Where(c => c.UserId == user.Id && c.CreatedAt > DateTime.UtcNow.AddSeconds(-MIN_RESEND_SECONDS))
                .FirstOrDefaultAsync();

            if (recentCode != null)
                return true; // Silenciosamente ignorar

            // Invalidar códigos activos anteriores del usuario
            var activeCodes = await _context.PasswordResetCode
                .Where(c => c.UserId == user.Id && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var old in activeCodes)
                old.IsUsed = true;

            // Generar código de 6 dígitos
            var plainCode = Random.Shared.Next(100_000, 999_999).ToString();
            var codeHash = PasswordHasher.HashPassword(plainCode);

            _context.PasswordResetCode.Add(new PasswordResetCode
            {
                UserId = user.Id,
                CodeHash = codeHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(CODE_TTL_MINUTES),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Enviar correo con el código en texto plano
            // Username es el correo del usuario
            await _emailService.EnviarEmailRecuperacion(user.Username, user.FullName, plainCode);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            // El Username es el correo electrónico del usuario
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedEmail);

            if (user == null)
                return false;

            var codeHash = PasswordHasher.HashPassword(code.Trim());

            var resetCode = await _context.PasswordResetCode
                .Where(c =>
                    c.UserId == user.Id &&
                    c.CodeHash == codeHash &&
                    !c.IsUsed &&
                    c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetCode == null)
                return false;

            // Marcar código como usado
            resetCode.IsUsed = true;

            // Actualizar contraseña
            user.PasswordHash = PasswordHasher.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
