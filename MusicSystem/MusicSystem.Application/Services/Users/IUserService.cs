using MusicSystem.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Users
{
    public interface IUserService
    {
        // Queries
        Task<IEnumerable<UserManagementDto>> GetAllUsersAsync();
        Task<UserManagementDto> GetUserByIdAsync(Guid userId);
        Task<IEnumerable<UserManagementDto>> GetUsersByRoleAsync(string roleName);

        // Commands
        Task<UserManagementDto> CreateUserAsync(CreateUserDto dto);
        Task<UserManagementDto> UpdateUserAsync(Guid userId, UpdateUserDto dto);
        Task<bool> DisableUserAsync(Guid userId);
        Task<bool> EnableUserAsync(Guid userId);
        Task<bool> AssignRoleAsync(Guid userId, string roleName);
        Task<bool> RemoveRoleAsync(Guid userId, string roleName);
        Task<bool> ResetPasswordAsync(Guid userId, string newPassword);
    }
}
