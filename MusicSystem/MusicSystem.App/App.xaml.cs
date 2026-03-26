using Microsoft.Extensions.DependencyInjection;
using MusicSystem.App.Services;
using MusicSystem.App.Views;
using MusicSystem.App.Views.Pages;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MusicSystem.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure DI
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Show login window
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // ===== SERVICES =====
            services.AddSingleton<ISocketClient>(sp =>
                new SocketClient("localhost", 1500));

            // ===== VIEWS =====
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();

            // ===== WINDOWS & DIALOGS =====
            services.AddTransient<AddUserWindow>();
            services.AddTransient<EditUserWindow>();
            services.AddTransient<AssignRoleWindow>();
            services.AddTransient<ResetPasswordDialog>();
            services.AddTransient<SelectArtistsDialog>();
            services.AddTransient<AddSongWindow>();
            services.AddTransient<EditSongWindow>();
            services.AddTransient<AddArtistWindow>();
            services.AddTransient<EditArtistWindow>();
            services.AddTransient<RejectReasonDialog>();

            // ===== MANAGEMENT PAGES (NEW) =====
            services.AddTransient<AdminDashboardPage>();
            services.AddTransient<DashboardPage>();
            services.AddTransient<PendingSongsPage>();
            services.AddTransient<SongManagementPage>();
            services.AddTransient<ArtistManagementPage>();
            services.AddTransient<UserManagementPage>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Cleanup
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }
    }



}


