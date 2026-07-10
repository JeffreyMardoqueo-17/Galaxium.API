using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Common;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;

namespace Galaxium.Api.Services.service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> AnyUserExistsAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users != null && users.Any();
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            if (users == null || !users.Any())
            {
                return Enumerable.Empty<User>();
                throw new NotFoundBusinessException("No users found.");
            }
            return users;
        }
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new NotFoundBusinessException("User not found.");
            return user;
        }

        public async Task<User> UpdateUserRoleAsync(int userId, int roleId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            if (roleId <= 0)
                throw new ArgumentException("Invalid role ID.");

            var updated = await _userRepository.UpdateUserRoleAsync(userId, roleId);
            if (updated == null)
                throw new NotFoundBusinessException("User not found.");

            return updated;
        }

        public async Task<User> UpdateUserStatusAsync(int userId, bool isActive)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            var updated = await _userRepository.UpdateUserStatusAsync(userId, isActive);
            if (updated == null)
                throw new NotFoundBusinessException("User not found.");

            return updated;
        }
    }
}