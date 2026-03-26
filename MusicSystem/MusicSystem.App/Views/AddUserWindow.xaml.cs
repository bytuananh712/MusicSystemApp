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
    /// Interaction logic for AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {

        private readonly ISocketClient _socketClient;

        public AddUserWindow(ISocketClient socketClient)
        {
            InitializeComponent();
            _socketClient = socketClient;
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Vui lòng nhập username");
                txtUsername.Focus();
                return;
            }

            var username = txtUsername.Text.Trim();
            if (username.Contains(' '))
            {
                ShowError("Username không được chứa khoảng trắng");
                txtUsername.Focus();
                return;
            }

            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    ShowError("Username chỉ được chứa chữ, số và dấu gạch dưới (_)");
                    txtUsername.Focus();
                    return;
                }
            }

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

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                ShowError("Vui lòng nhập mật khẩu");
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Password.Length < 6)
            {
                ShowError("Mật khẩu phải có ít nhất 6 ký tự");
                txtPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
            {
                ShowError("Vui lòng xác nhận mật khẩu");
                txtConfirmPassword.Focus();
                return;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                ShowError("Mật khẩu xác nhận không khớp");
                txtConfirmPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("Vui lòng nhập họ tên");
                txtFullName.Focus();
                return;
            }

            // Create DTO (luôn là Customer)
            var createDto = new CreateUserDto
            {
                Username = username,
                Email = email,
                Password = txtPassword.Password,
                FullName = txtFullName.Text.Trim(),
                RoleNames = new List<string> { "Customer" }
            };

            // Save
            await SaveUserAsync(createDto);
        }

        private async Task SaveUserAsync(CreateUserDto dto)
        {
            try
            {
                btnSave.IsEnabled = false;
                btnCancel.IsEnabled = false;
                this.Cursor = System.Windows.Input.Cursors.Wait;

                var request = new SocketRequest
                {
                    Command = SocketCommands.CreateUser,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = JsonSerializer.Serialize(dto)
                };

                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    MessageBox.Show(
                        "Thêm người dùng thành công!",
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
