using Microsoft.Extensions.DependencyInjection;
using MusicSystem.App.Services;
using MusicSystem.App.Views;
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


            // Windows
            services.AddTransient<AddUserWindow>();
            services.AddTransient<EditUserWindow>();
            services.AddTransient<AssignRoleWindow>();
            services.AddTransient<ResetPasswordDialog>();


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


