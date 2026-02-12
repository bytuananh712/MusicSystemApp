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
    }
}
