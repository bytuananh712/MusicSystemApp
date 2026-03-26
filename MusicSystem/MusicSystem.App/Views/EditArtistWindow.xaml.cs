using MusicSystem.App.Services;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Artists;
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
    /// Interaction logic for EditArtistWindow.xaml
    /// </summary>
    public partial class EditArtistWindow : Window
    {

        private readonly ISocketClient _socketClient;
        private readonly ArtistDto _artist;
        public EditArtistWindow(ISocketClient socketClient, ArtistDto artist)
        {
            InitializeComponent();

            _socketClient = socketClient;
            _artist = artist;

            LoadArtistData();
        }




        private void LoadArtistData()
        {
            txtArtistName.Text = $"Nghệ sĩ: {_artist.ArtistName}";
            txtNewArtistName.Text = _artist.ArtistName;
            txtBiography.Text = _artist.Biography;
            txtAvatarUrl.Text = _artist.AvatarUrl;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtNewArtistName.Text))
            {
                ShowError("Vui lòng nhập tên nghệ sĩ");
                txtNewArtistName.Focus();
                return;
            }

            if (txtNewArtistName.Text.Trim().Length > 100)
            {
                ShowError("Tên nghệ sĩ không được vượt quá 100 ký tự");
                txtNewArtistName.Focus();
                return;
            }

            var avatarUrl = txtAvatarUrl.Text.Trim();
            if (!string.IsNullOrEmpty(avatarUrl)
                && !avatarUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !avatarUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("URL ảnh đại diện phải bắt đầu bằng http:// hoặc https://");
                txtAvatarUrl.Focus();
                return;
            }

            // Create DTO
            var updateDto = new UpdateArtistDto
            {
                ArtistName = txtNewArtistName.Text.Trim(),
                Biography = txtBiography.Text.Trim(),
                AvatarUrl = txtAvatarUrl.Text.Trim()
            };

            await UpdateArtistAsync(updateDto);
        }

        private async Task UpdateArtistAsync(UpdateArtistDto dto)
        {
            try
            {
                btnSave.IsEnabled = false;
                btnCancel.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                var data = new Dictionary<string, object>
                {
                    { "artistId", _artist.ArtistId.ToString() },
                    { "data", dto }
                };

                var request = new SocketRequest
                {
                    Command = SocketCommands.UpdateArtist,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = JsonSerializer.Serialize(data)
                };

                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    MessageBox.Show(
                        "Cập nhật thông tin nghệ sĩ thành công!",
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
                Mouse.OverrideCursor = null;
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }









    }
}
