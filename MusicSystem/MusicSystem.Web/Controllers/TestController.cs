using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSystem.Application.Services.Auth;
using MusicSystem.Infrastructure.Data;
using MusicSystem.Shared.DTOs.Auth;
using MusicSystem.Application.Services.Auth;


namespace MusicSystem.Web.Controllers
{
    public class TestController : Controller
    {
        private readonly MusicStreamingDbContext _context;
        private readonly IAuthService _authService;

        public TestController(MusicStreamingDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;

        }

        // GET: /Test/TestLogin
        public async Task<IActionResult> TestLogin()
        {
            var loginRequest = new LoginRequestDto
            {
                Username = "admin",
                Password = "Admin@123"
            };

            var result = await _authService.LoginAsync(loginRequest);

            return Json(result);
        }







        // GET: /Test/CheckDb
        public async Task<IActionResult> CheckDb()
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                var roleCount = await _context.Roles.CountAsync();
                var songCount = await _context.Songs.CountAsync();

                var result = $"✅ Connected to Database!\n\n" +
                            $"Users: {userCount}\n" +
                            $"Roles: {roleCount}\n" +
                            $"Songs: {songCount}";

                return Content(result, "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error: {ex.Message}", "text/plain");
            }
        }

        // GET: /Test/ShowUsers
        public async Task<IActionResult> ShowUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new {
                    
                    u.Username,
                    u.Email,
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName) // Chỉ lấy tên Role
                })
                .ToListAsync();

            return Json(users);
        }


        // GET: /Test/HashPassword
        public IActionResult HashPassword(string password = "Customer@123")
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            return Content($"Password: {password}\nHash: {hash}");
        }






    }
}