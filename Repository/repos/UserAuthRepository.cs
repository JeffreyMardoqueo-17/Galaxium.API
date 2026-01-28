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

        public async Task<User> CreateUserAsync(User newUser)
        {
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }

        public async Task<User?> AuthenticateUserAsync(string username)
        {
            return await _context.User
                .Include(u => u.Role)  // <== Carga explícita del rol
                .FirstOrDefaultAsync(u => u.Username == username);
        }


    }
}
