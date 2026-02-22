using MusicSystem.App.Services;
using MusicSystem.App.Views; // ← Import để dùng các Window
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Users;
using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; 
namespace MusicSystem.App.Views.Pages 
{
    public partial class UserManagementPage : Page
    {
        private readonly ISocketClient _socketClient;
        private List<UserManagementDto> _allUsers;
        private List<UserManagementDto> _filteredUsers;

        public UserManagementPage(ISocketClient socketClient)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _allUsers = new List<UserManagementDto>();
            _filteredUsers = new List<UserManagementDto>();

            // Load data khi page load
            Loaded += UserManagementPage_Loaded; 
        }

        private async void UserManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        // ==================== LOAD DATA ====================
        private async Task LoadUsersAsync()
        {
            try
            {
                
                Mouse.OverrideCursor = Cursors.Wait;
                btnRefresh.IsEnabled = false;

                // Tạo request
                var request = new SocketRequest
                {
                    Command = SocketCommands.GetAllUsers,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = "{}"
                };

                // Gửi request
                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    // Parse response
                    _allUsers = JsonSerializer.Deserialize<List<UserManagementDto>>(response.Data);
                    _filteredUsers = _allUsers.ToList();

                    // Bind to DataGrid
                    dgUsers.ItemsSource = _filteredUsers;

                    // Update status
                    txtStatus.Text = $"Tổng: {_allUsers.Count} người dùng";
                }
                else
                {
                    MessageBox.Show(
                        $"Lỗi: {response.Message}",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải danh sách người dùng:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // ✅ Sửa: Reset cursor
                Mouse.OverrideCursor = null;
                btnRefresh.IsEnabled = true;
            }
        }

        // ==================== SEARCH ====================
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var searchTerm = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                _filteredUsers = _allUsers.ToList();
            }
            else
            {
                _filteredUsers = _allUsers.Where(u =>
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FullName.ToLower().Contains(searchTerm)
                ).ToList();
            }

            dgUsers.ItemsSource = null;
            dgUsers.ItemsSource = _filteredUsers;
            txtStatus.Text = $"Tìm thấy: {_filteredUsers.Count}/{_allUsers.Count} người dùng";
        }

        // ==================== REFRESH ====================
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            await LoadUsersAsync();
        }

        // ==================== ADD USER ====================
        private async void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddUserWindow(_socketClient);
            // ✅ Sửa: Dùng Window.GetWindow(this)
            addWindow.Owner = Window.GetWindow(this);

            var result = addWindow.ShowDialog();

            if (result == true)
            {
                await LoadUsersAsync();
            }
        }

        // ==================== EDIT USER ====================
        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid userId)
            {
                var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return;

                var editWindow = new EditUserWindow(_socketClient, user);
                // ✅ Sửa
                editWindow.Owner = Window.GetWindow(this);

                var result = editWindow.ShowDialog();

                if (result == true)
                {
                    await LoadUsersAsync();
                }
            }
        }

        // ==================== DISABLE USER ====================
        private async void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid userId)
            {
                var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return;

                var confirm = MessageBox.Show(
                    $"Bạn có chắc muốn vô hiệu hóa tài khoản '{user.Username}'?\n\n" +
                    $"Người dùng này sẽ không thể đăng nhập vào hệ thống.",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes)
                    return;

                try
                {
                    var request = new SocketRequest
                    {
                        Command = SocketCommands.DisableUser,
                        Token = Application.Current.Properties["AuthToken"]?.ToString(),
                        Data = userId.ToString()
                    };

                    var response = await _socketClient.SendRequestAsync(request);

                    if (response.Status == SocketStatus.Success)
                    {
                        MessageBox.Show(
                            $"Đã vô hiệu hóa tài khoản '{user.Username}'",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadUsersAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Lỗi: {response.Message}",
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Lỗi: {ex.Message}",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        // ==================== ENABLE USER ====================
        private async void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid userId)
            {
                var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return;

                try
                {
                    var request = new SocketRequest
                    {
                        Command = SocketCommands.EnableUser,
                        Token = Application.Current.Properties["AuthToken"]?.ToString(),
                        Data = userId.ToString()
                    };

                    var response = await _socketClient.SendRequestAsync(request);

                    if (response.Status == SocketStatus.Success)
                    {
                        MessageBox.Show(
                            $"Đã kích hoạt tài khoản '{user.Username}'",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadUsersAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Lỗi: {response.Message}",
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Lỗi: {ex.Message}",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        // ==================== ASSIGN ROLE ====================
        private async void btnAssignRole_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid userId)
            {
                var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return;

                var assignRoleWindow = new AssignRoleWindow(_socketClient, user);
                // ✅ Sửa
                assignRoleWindow.Owner = Window.GetWindow(this);

                var result = assignRoleWindow.ShowDialog();

                if (result == true)
                {
                    await LoadUsersAsync();
                }
            }
        }

        // ==================== RESET PASSWORD ====================
        private async void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid userId)
            {
                var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return;

                var inputDialog = new ResetPasswordDialog(user.Username);
                // ✅ Sửa
                inputDialog.Owner = Window.GetWindow(this);

                var result = inputDialog.ShowDialog();

                if (result == true)
                {
                    var newPassword = inputDialog.NewPassword;

                    try
                    {
                        var data = new Dictionary<string, string>
                        {
                            { "userId", userId.ToString() },
                            { "newPassword", newPassword }
                        };

                        var request = new SocketRequest
                        {
                            Command = SocketCommands.ResetPassword,
                            Token = Application.Current.Properties["AuthToken"]?.ToString(),
                            Data = JsonSerializer.Serialize(data)
                        };

                        var response = await _socketClient.SendRequestAsync(request);

                        if (response.Status == SocketStatus.Success)
                        {
                            MessageBox.Show(
                                $"Đã reset mật khẩu cho '{user.Username}'\n\nMật khẩu mới: {newPassword}",
                                "Thành công",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                $"Lỗi: {response.Message}",
                                "Lỗi",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Lỗi: {ex.Message}",
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}