using MusicSystem.App.Services;
using MusicSystem.App.Views.Pages;
using MusicSystem.Shared.DTOs.Auth;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicSystem.App.Views
{
    public partial class MainWindow : Window
    {
        private readonly ISocketClient _socketClient;
        private UserDto _currentUser;
        private Button _activeMenuButton;

        public MainWindow(ISocketClient socketClient)
        {
            InitializeComponent();
            _socketClient = socketClient;

            LoadUserInfo();
            SetupMenuByRole();

            // Load Dashboard mặc định
            LoadDashboard();
        }

        // ==================== LOAD USER INFO ====================
        private void LoadUserInfo()
        {
            if (Application.Current.Properties["CurrentUser"] is UserDto user)
            {
                _currentUser = user;

                // Header info
                txtWelcome.Text = user.FullName ?? user.Username;
                txtRole.Text = string.Join(", ", user.Roles);

                // Avatar initial (chữ cái đầu)
                txtAvatarInitial.Text = (user.FullName ?? user.Username).Substring(0, 1).ToUpper();

                // Subtitle
                if (user.Roles.Contains("Admin"))
                    txtSubtitle.Text = "Admin Portal - Quản trị hệ thống";
                else if (user.Roles.Contains("Manager"))
                    txtSubtitle.Text = "Manager Portal - Quản lý nội dung";
            }
        }

        // ==================== SETUP MENU BY ROLE ====================
        private void SetupMenuByRole()
        {
            if (_currentUser == null) return;

            // Admin menu
            if (_currentUser.Roles.Contains("Admin"))
            {
                adminMenu.Visibility = Visibility.Visible;
            }

            // Manager menu
            if (_currentUser.Roles.Contains("Manager"))
            {
                managerMenu.Visibility = Visibility.Visible;
            }
        }

        // ==================== NAVIGATION ====================
        private void SetActiveMenu(Button button)
        {
            // Reset previous active button
            if (_activeMenuButton != null)
            {
                _activeMenuButton.Background = System.Windows.Media.Brushes.Transparent;
            }

            // Set new active button
            _activeMenuButton = button;
            if (button != null)
            {
                button.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(26, 188, 156)); // #FF1ABC9C
            }
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnDashboard);
            LoadDashboard();
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnUsers);
            LoadUserManagement();
        }

        // ✅ THÊM CÁC EVENT HANDLER THIẾU
        private void btnSongs_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnSongs);
            LoadSongManagement();
        }

        private void btnArtists_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnArtists);
            LoadArtistManagement();
        }

        private void btnPendingSongs_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnPendingSongs);
            LoadPendingSongs();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(btnSettings);
            LoadSettings();
        }

        // ==================== LOAD PAGES ====================
        private void LoadDashboard()
        {
            var dashboardPage = new DashboardPage(_socketClient, _currentUser);
            mainFrame.Navigate(dashboardPage);
        }

        private void LoadUserManagement()
        {
            var userPage = new UserManagementPage(_socketClient);
            mainFrame.Navigate(userPage);
        }




        private void LoadSongManagement()
        {
            // TODO: Tạo SongManagementPage sau
            MessageBox.Show(
                "Tính năng Quản lý bài hát đang được phát triển",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void LoadArtistManagement()
        {
            // TODO: Tạo ArtistManagementPage sau
            MessageBox.Show(
                "Tính năng Quản lý nghệ sĩ đang được phát triển",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void LoadPendingSongs()
        {
            // TODO: Tạo PendingSongsPage sau
            MessageBox.Show(
                "Tính năng Duyệt bài hát đang được phát triển",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void LoadSettings()
        {
            MessageBox.Show(
                "Tính năng Cài đặt đang được phát triển",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // ==================== LOGOUT ====================
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear session
                Application.Current.Properties.Remove("AuthToken");
                Application.Current.Properties.Remove("CurrentUser");

                // Disconnect socket
                _socketClient.Disconnect();

                // Open LoginWindow
                var loginWindow = new LoginWindow(_socketClient);
                loginWindow.Show();

                // Close MainWindow
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Nếu đóng mà chưa logout → shutdown app
            if (Application.Current.Properties.Contains("CurrentUser"))
            {
                Application.Current.Shutdown();
            }
        }
    }
}