using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Data;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GalaxiumERP.API.Repository.repos
{
    public class RolRepository : IRoleRespository
    {
        private readonly GalaxiumDbContext _context;
        public RolRepository(GalaxiumDbContext context)
        {
            _context = context;
        }

        //metodos para manejar roles bueno traelos

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Role
                        .AsNoTracking()
                        .ToListAsync();
        }
        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Role
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.Id == roleId);
        }
    }
}