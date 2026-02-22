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

namespace MusicSystem.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<MusicStreamingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IArtistRepository, ArtistRepository>();  // Quản lí nghệ sĩ
            builder.Services.AddScoped<ISongRepository, SongRepository>();  // Quản lí bài hát

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
