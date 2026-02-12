using MusicSystem.App.Services;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Users;
using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicSystem.App.Views
{
    /// <summary>
    /// Interaction logic for AssignRoleWindow.xaml
    /// </summary>
    public partial class AssignRoleWindow : Window
    {

        private readonly ISocketClient _socketClient;
        private readonly UserManagementDto _user;

        public AssignRoleWindow(ISocketClient socketClient, UserManagementDto user)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _user = user;

            LoadUserRoles();
        }

        private void LoadUserRoles()
        {
            txtUserInfo.Text = $"Username: {_user.Username} | Email: {_user.Email}";

            // Check current roles
            chkAdmin.IsChecked = _user.Roles.Contains("Admin");
            chkManager.IsChecked = _user.Roles.Contains("Manager");
            chkCustomer.IsChecked = _user.Roles.Contains("Customer");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate: Phải chọn ít nhất 1 role
            if (chkAdmin.IsChecked != true &&
                chkManager.IsChecked != true &&
                chkCustomer.IsChecked != true)
            {
                MessageBox.Show(
                    "Vui lòng chọn ít nhất 1 vai trò",
                    "Cảnh báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            await SaveRolesAsync();
        }

        private async Task SaveRolesAsync()
        {
            try
            {
                btnSave.IsEnabled = false;
                btnCancel.IsEnabled = false;
                this.Cursor = System.Windows.Input.Cursors.Wait;

                // Remove all current roles first (để đơn giản)
                foreach (var role in _user.Roles)
                {
                    await RemoveRoleAsync(role);
                }

                // Add selected roles
                if (chkAdmin.IsChecked == true)
                    await AssignRoleAsync("Admin");

                if (chkManager.IsChecked == true)
                    await AssignRoleAsync("Manager");

                if (chkCustomer.IsChecked == true)
                    await AssignRoleAsync("Customer");

                MessageBox.Show(
                    "Cập nhật quyền thành công!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnSave.IsEnabled = true;
                btnCancel.IsEnabled = true;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private async Task AssignRoleAsync(string roleName)
        {
            var dto = new AssignRoleDto
            {
                UserId = _user.UserId,
                RoleName = roleName
            };

            var request = new SocketRequest
            {
                Command = SocketCommands.AssignRole,
                Token = Application.Current.Properties["AuthToken"]?.ToString(),
                Data = JsonSerializer.Serialize(dto)
            };

            await _socketClient.SendRequestAsync(request);
        }

        private async Task RemoveRoleAsync(string roleName)
        {
            var dto = new AssignRoleDto
            {
                UserId = _user.UserId,
                RoleName = roleName
            };

            var request = new SocketRequest
            {
                Command = SocketCommands.RemoveRole,
                Token = Application.Current.Properties["AuthToken"]?.ToString(),
                Data = JsonSerializer.Serialize(dto)
            };

            await _socketClient.SendRequestAsync(request);
        }



    }
}
