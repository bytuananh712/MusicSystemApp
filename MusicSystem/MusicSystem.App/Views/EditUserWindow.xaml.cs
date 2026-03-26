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
    /// Interaction logic for EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {


        private readonly ISocketClient _socketClient;
        private readonly UserManagementDto _user;

        public EditUserWindow(ISocketClient socketClient, UserManagementDto user)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _user = user;

            LoadUserData();
        }

        private void LoadUserData()
        {
            txtUsername.Text = $"Username: {_user.Username}";
            txtEmail.Text = _user.Email;
            txtFullName.Text = _user.FullName;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowError("Vui lòng nhập email");
                txtEmail.Focus();
                return;
            }

            var email = txtEmail.Text.Trim();
            if (!email.Contains('@') || !email.Contains('.') || email.IndexOf('@') > email.LastIndexOf('.'))
            {
                ShowError("Email không đúng định dạng (ví dụ: user@example.com)");
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("Vui lòng nhập họ tên");
                txtFullName.Focus();
                return;
            }

            // Create DTO
            var updateDto = new UpdateUserDto
            {
                Email = email,
                FullName = txtFullName.Text.Trim()
            };

            await UpdateUserAsync(updateDto);
        }

        private async Task UpdateUserAsync(UpdateUserDto dto)
        {
            try
            {
                btnSave.IsEnabled = false;
                btnCancel.IsEnabled = false;
                this.Cursor = System.Windows.Input.Cursors.Wait;

                var data = new Dictionary<string, object>
                {
                    { "userId", _user.UserId.ToString() },
                    { "data", dto }
                };

                var request = new SocketRequest
                {
                    Command = SocketCommands.UpdateUser,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = JsonSerializer.Serialize(data)
                };

                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    MessageBox.Show(
                        "Cập nhật thông tin thành công!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError(response.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
            }
            finally
            {
                btnSave.IsEnabled = true;
                btnCancel.IsEnabled = true;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }




    }
}
