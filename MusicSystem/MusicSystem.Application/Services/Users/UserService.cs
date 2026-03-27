using MusicSystem.Application.Services.Auth;
using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuthService _authService;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IAuthService authService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _authService = authService;
        }

        public async Task<IEnumerable<UserManagementDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserManagementDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception($"User with ID {userId} not found");

            return MapToDto(user);
        }

        public async Task<IEnumerable<UserManagementDto>> GetUsersByRoleAsync(string roleName)
        {
            var users = await _userRepository.GetUsersByRoleAsync(roleName);
            return users.Select(MapToDto);
        }

        public async Task<UserManagementDto> CreateUserAsync(CreateUserDto dto)
        {
            // Check username exists
            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
                throw new Exception("Username đã tồn tại");

            // Check email exists
            existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email đã tồn tại");

            // Create user
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _authService.HashPassword(dto.Password),
                FullName = dto.FullName,
                Avatar = dto.Avatar ?? $"https://ui-avatars.com/api/?name={dto.FullName}&background=random",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Assign roles
            foreach (var roleName in dto.RoleNames)
            {
                await AssignRoleAsync(user.UserId, roleName);
            }

            return MapToDto(user);
        }

        public async Task<UserManagementDto> UpdateUserAsync(Guid userId, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception($"User with ID {userId} not found");

            // Check duplicate email
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var emailExists = await _userRepository.GetByEmailAsync(dto.Email);
                if (emailExists != null)
                    throw new Exception("Email này đã được sử dụng bởi tài khoản khác!");
            }

            user.Email = dto.Email ?? user.Email;
            user.FullName = dto.FullName ?? user.FullName;
            user.Avatar = dto.Avatar ?? user.Avatar;

            await _userRepository.UpdateAsync(user);
            return MapToDto(user);
        }

        public async Task<bool> DisableUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.Status = "Disabled";
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> EnableUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.Status = "Active";
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> AssignRoleAsync(Guid userId, string roleName)
        {
            var role = await _roleRepository.GetByNameAsync(roleName);
            if (role == null)
                throw new Exception($"Role '{roleName}' not found");

            return await _userRepository.AssignRoleAsync(userId, role.RoleId);
        }

        public async Task<bool> RemoveRoleAsync(Guid userId, string roleName)
        {
            var role = await _roleRepository.GetByNameAsync(roleName);
            if (role == null) return false;

            return await _userRepository.RemoveRoleAsync(userId, role.RoleId);
        }

        public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.PasswordHash = _authService.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        // Helper
        private UserManagementDto MapToDto(User user)
        {
            return new UserManagementDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Avatar = user.Avatar,
                Status = user.Status,
                Roles = user.UserRoles?.Select(ur => ur.Role?.RoleName).ToList() ?? new List<string>(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
