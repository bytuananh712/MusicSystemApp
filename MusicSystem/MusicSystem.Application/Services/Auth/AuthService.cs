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
            // Token : Base64(UserId + Timestamp)
            var tokenData = $"{userId}:{DateTime.UtcNow.Ticks}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(bytes);
        }

        
        /// Giải mã Simple Token và trả về UserId nếu hợp lệ.
        /// Token format: Base64("UserId:Ticks")
        /// Token hết hạn sau 24 giờ.
       
        public Guid? ValidateSimpleToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var bytes = Convert.FromBase64String(token);
                var decoded = System.Text.Encoding.UTF8.GetString(bytes);

                // Format: "UserId:Ticks"
                var parts = decoded.Split(':');
                if (parts.Length != 2)
                    return null;

                if (!Guid.TryParse(parts[0], out var userId))
                    return null;

                if (!long.TryParse(parts[1], out var ticks))
                    return null;

                // Kiểm tra hạn token (24 giờ)
                var tokenTime = new DateTime(ticks, DateTimeKind.Utc);
                if ((DateTime.UtcNow - tokenTime).TotalHours > 24)
                    return null; // Token hết hạn

                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}