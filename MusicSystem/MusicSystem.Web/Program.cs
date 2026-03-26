using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicSystem.Application.Services.Artists;
using MusicSystem.Application.Services.Auth;
using MusicSystem.Application.Services.Files;
using MusicSystem.Application.Services.History;
using MusicSystem.Application.Services.Likes;
using MusicSystem.Application.Services.Playlists;
using MusicSystem.Application.Services.Songs;
using MusicSystem.Application.Services.Users;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Infrastructure.Data;
using MusicSystem.Infrastructure.Repositories;
using MusicSystem.Infrastructure.Socket;
using Serilog;
using Serilog.Events;

namespace MusicSystem.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== SERILOG CONFIGURATION =====
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  //  FIX: Warning instead of Information
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/app-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7)  //  Keep logs for 7 days
                .CreateLogger();

            builder.Host.UseSerilog();

            // ===== ADD SERVICES =====
            builder.Services.AddControllersWithViews();

            // ===== DATABASE CONTEXT =====
            // ✅ ĐƠN GIẢN - Không có UseQuerySplittingBehavior
            builder.Services.AddDbContext<MusicStreamingDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // ===== REPOSITORIES (Domain Interfaces) =====
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
            builder.Services.AddScoped<ISongRepository, SongRepository>();
            builder.Services.AddScoped<ILikeRepository, LikeRepository>();
            builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
            builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();

            // ===== SERVICES (Application Layer) =====
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IArtistService, ArtistService>();
            builder.Services.AddScoped<ISongService, SongService>();
            builder.Services.AddScoped<IPlaylistService, PlaylistService>();
            builder.Services.AddScoped<ILikeService, LikeService>();
            builder.Services.AddScoped<IHistoryService, HistoryService>();

            // ===== SOCKET SERVER =====
            builder.Services.AddScoped<SocketHandler>();
            builder.Services.AddHostedService<SocketServer>();

            // ===== FILE UPLOAD SERVICE =====
            var uploadPath = Path.Combine(builder.Environment.WebRootPath, "uploads", "songs");

            //  FIX: Dùng Singleton thay vì Scoped (path không đổi)
            builder.Services.AddSingleton<IFileUploadService>(
                new FileUploadService(uploadPath)
            );

            // ===== AUTHENTICATION =====
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(24);
                    options.SlidingExpiration = true;
                    options.Cookie.Name = "MusicStreamingAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

            // ===== SESSION =====
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();

            // ===== BUILD APP =====
            var app = builder.Build();

            // ===== CREATE UPLOAD FOLDER =====
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // ===== HTTP PIPELINE =====
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}