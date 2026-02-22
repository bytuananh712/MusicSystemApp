using MusicSystem.App.Services;
using MusicSystem.Shared.DTOs.Auth;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicSystem.App.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly ISocketClient _socketClient;
        private readonly UserDto _currentUser;

        public DashboardPage(ISocketClient socketClient, UserDto currentUser)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _currentUser = currentUser;

            LoadDashboard();
        }

        private void LoadDashboard()
        {
            // Welcome message
            txtWelcome.Text = $"Xin chào, {_currentUser.FullName ?? _currentUser.Username}!";

            // Show cards based on role
            if (_currentUser.Roles.Contains("Admin"))
            {
                cardUsers.Visibility = Visibility.Visible;
                adminQuickActions.Visibility = Visibility.Visible;
                // TODO: Load user count
            }

            if (_currentUser.Roles.Contains("Manager"))
            {
                cardSongs.Visibility = Visibility.Visible;
                cardArtists.Visibility = Visibility.Visible;
                cardPending.Visibility = Visibility.Visible;
                managerQuickActions.Visibility = Visibility.Visible;
                // TODO: Load statistics
            }
        }

        // Quick Actions
        private void btnQuickAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddUserWindow(_socketClient);
            addWindow.Owner = Window.GetWindow(this);
            addWindow.ShowDialog();
        }

        private void btnQuickAddSong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng đang phát triển", "Thông báo");
        }

        private void btnQuickAddArtist_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng đang phát triển", "Thông báo");
        }

        private void btnQuickPending_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng đang phát triển", "Thông báo");
        }
    }
}