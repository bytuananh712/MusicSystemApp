using MusicSystem.Shared.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Auth
{
    public interface IAuthService  // kiểm tra đăng nhập 
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);

        
        /// Giải mã Simple Token (Base64(UserId:Ticks)) và trả về UserId.
        /// Trả về null nếu Token không hợp lệ.
        
        Guid? ValidateSimpleToken(string token);
    }
}
