using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Galaxium.API.Data;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.API.Repository.Repos
{
    public class UserAuthRepository : IUserAuthRepository
    {
        private readonly GalaxiumDbContext _context;

        public UserAuthRepository(GalaxiumDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Crea un nuevo usuario y retorna la entidad con el rol cargado.
        /// </summary>
        /// <param name="newUser">Entidad usuario a crear.</param>
        /// <returns>Usuario creado con la propiedad Role cargada.</returns>
        public async Task<User> CreateUserAsync(User newUser)
        {
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();
            // Recargar el usuario con el rol incluido para evitar roleName null
            return await _context.User.Include(u => u.Role).FirstAsync(u => u.Id == newUser.Id);
        }

        public async Task<User?> AuthenticateUserAsync(string username)
        {
            return await _context.User
                .Include(u => u.Role)  // <== Carga explícita del rol
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId)
        {
            return await _context.RefreshToken
                .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }



    }
}
