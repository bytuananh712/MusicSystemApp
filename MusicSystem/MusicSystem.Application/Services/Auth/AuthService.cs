using MusicSystem.Application.Services.Auth;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Shared.DTOs.Auth;
using MusicSystem.Application.Services.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            // Tìm user theo username hoặc email
            var user = await _userRepository.GetByUsernameAsync(loginRequest.Username);

            if (user == null)
            {
                // Thử tìm theo email
                user = await _userRepository.GetByEmailAsync(loginRequest.Username);
            }

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                };
            }

            // Kiểm tra trạng thái tài khoản
            if (user.Status != "Active")
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Tài khoản đã bị vô hiệu hóa"
                };
            }

            // Kiểm tra mật khẩu
            if (!VerifyPassword(loginRequest.Password, user.PasswordHash))  // Kiểm tra mk nhập và trong db
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                };
            }

            // Cập nhật thời gian đăng nhập
            await _userRepository.UpdateLastLoginAsync(user.UserId);

            // Map sang DTO
            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Avatar = user.Avatar,
                Roles = user.UserRoles?.Select(ur => ur.Role.RoleName).ToList() ?? new List<string>()
            };

            // Tạo token (đơn giản)
            var token = GenerateSimpleToken(user.UserId);

            return new LoginResponseDto
            {
                Success = true,
                Message = "Đăng nhập thành công",
                User = userDto,
                Token = token
            };
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateSimpleToken(Guid userId)
        {
            // Token đơn giản: Base64(UserId + Timestamp)
            var tokenData = $"{userId}:{DateTime.UtcNow.Ticks}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(bytes);
        }
    }
}