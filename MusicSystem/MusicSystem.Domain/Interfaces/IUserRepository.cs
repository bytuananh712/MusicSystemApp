using MusicSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(Guid userId);
        Task<bool> UpdateLastLoginAsync(Guid userId);

        // Thêm các method mới
        Task<IEnumerable<User>> GetAllWithRolesAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> AssignRoleAsync(Guid userId, Guid roleId);
        Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);


    }
}
