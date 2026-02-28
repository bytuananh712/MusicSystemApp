using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicSystem.Application.Services.Artists;
using MusicSystem.Application.Services.Auth;
using MusicSystem.Application.Services.Auth;
using MusicSystem.Application.Services.Files;
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


            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)             
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) 
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/app-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            builder.Host.UseSerilog();


            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<MusicStreamingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IArtistRepository, ArtistRepository>();  // Quản lí nghệ sĩ
            builder.Services.AddScoped<ISongRepository, SongRepository>();  // Quản lí bài hát

            builder.Services.AddScoped<ILikeRepository, LikeRepository>();          // Nhạc yêu thích
            builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();    // Lịch sử nghe
            builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();  // Danh sách phát 


            // Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IArtistService, ArtistService>();
            builder.Services.AddScoped<ISongService, SongService>();


            // ===== SOCKET SERVER =====
            builder.Services.AddScoped<SocketHandler>(); // Scoped per client connection
            builder.Services.AddHostedService<SocketServer>(); // Background service

            // ===== FILE UPLOAD SERVICE =====
            
            var uploadPath = Path.Combine(builder.Environment.WebRootPath, "uploads", "songs");
            builder.Services.AddScoped<IFileUploadService>(sp => new FileUploadService(uploadPath));

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


            // Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();




            var app = builder.Build();

            var uploadsFolder = Path.Combine(app.Environment.WebRootPath, "uploads", "songs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }



            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Thêm session
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
