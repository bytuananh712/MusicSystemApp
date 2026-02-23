using MusicSystem.App.Services;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Auth;
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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly ISocketClient _socketClient;

        public LoginWindow(ISocketClient socketClient)
        {
            InitializeComponent();
            _socketClient = socketClient;

            // Set default password cho demo
            txtPassword.Password = "Manager@123";

            // Focus vào username
            txtUsername.Focus();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            await LoginAsync();
        }

        private async void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter để login
            if (e.Key == Key.Enter)
            {
                await LoginAsync();
            }
        }

        private async Task LoginAsync()
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Vui lòng nhập username hoặc email");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                ShowError("Vui lòng nhập mật khẩu");
                txtPassword.Focus();
                return;
            }

            // Show loading
            SetLoading(true);
            HideError();

            try
            {
                // Tạo login DTO
                var loginDto = new LoginRequestDto
                {
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Password
                };

                // Tạo socket request
                var request = new SocketRequest
                {
                    RequestId = Guid.NewGuid(),
                    Command = SocketCommands.Login,
                    Token = string.Empty,
                    Data = JsonSerializer.Serialize(loginDto),
                    Timestamp = DateTime.UtcNow
                };

                // Gửi request qua socket
                var response = await _socketClient.SendRequestAsync(request);

                // Xử lý response
                if (response.Status == SocketStatus.Success)
                {
                    // Parse login result
                    var loginResult = JsonSerializer.Deserialize<LoginResponseDto>(response.Data);

                    // Lưu thông tin user vào Application Properties
                    Application.Current.Properties["AuthToken"] = loginResult.Token;
                    Application.Current.Properties["CurrentUser"] = loginResult.User;

                    // Log thông tin
                    System.Diagnostics.Debug.WriteLine($" Login thành công: {loginResult.User.FullName}");
                    System.Diagnostics.Debug.WriteLine($" Roles: {string.Join(", ", loginResult.User.Roles)}");

                    // Mở MainWindow
                    var mainWindow = new MainWindow(_socketClient);
                    mainWindow.Show();

                    // Đóng LoginWindow
                    this.Close();
                }
                else if (response.Status == SocketStatus.Unauthorized)
                {
                    ShowError(response.Message);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
                else
                {
                    ShowError($"Lỗi: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Không thể kết nối tới server:\n{ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            errorBorder.Visibility = Visibility.Collapsed;
            txtError.Text = string.Empty;
        }

        private void SetLoading(bool isLoading)
        {
            btnLogin.IsEnabled = !isLoading;
            txtUsername.IsEnabled = !isLoading;
            txtPassword.IsEnabled = !isLoading;
            loadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            if (isLoading)
            {
                btnLogin.Content = "Đang đăng nhập...";
                btnLogin.Background = System.Windows.Media.Brushes.Gray;
                this.Cursor = Cursors.Wait;
            }
            else
            {
                btnLogin.Content = "ĐĂNG NHẬP";
                btnLogin.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF3498DB");
                this.Cursor = Cursors.Arrow;
            }
        }
    }
}
