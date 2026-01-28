using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Data;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.API.Repository.Repos
{
    public class UserRepository : IUserRepository
    {
        private readonly GalaxiumDbContext _context;

        public UserRepository(GalaxiumDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await Task.FromResult(_context.User.ToList());
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = _context.User.FirstOrDefault(u => u.Id == userId);
            return await Task.FromResult(user);
        }
    }
}