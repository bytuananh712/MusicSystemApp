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
    /// Interaction logic for AddArtistWindow.xaml
    /// </summary>
    public partial class AddArtistWindow : Window
    {

        private readonly ISocketClient _socketClient;
        public AddArtistWindow(ISocketClient socketClient)
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
            if (string.IsNullOrWhiteSpace(txtArtistName.Text))
            {
                ShowError("Vui lòng nhập tên nghệ sĩ");
                txtArtistName.Focus();
                return;
            }

            // Create DTO
            var createDto = new CreateArtistDto
            {
                ArtistName = txtArtistName.Text.Trim(),
                Biography = txtBiography.Text.Trim(),
                AvatarUrl = txtAvatarUrl.Text.Trim()
            };

            // Save
            await SaveArtistAsync(createDto);
        }

        private async Task SaveArtistAsync(CreateArtistDto dto)
        {
            try
            {
                btnSave.IsEnabled = false;
                btnCancel.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                var request = new SocketRequest
                {
                    Command = SocketCommands.CreateArtist,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = JsonSerializer.Serialize(dto)
                };

                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    MessageBox.Show(
                        "Thêm nghệ sĩ thành công!",
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
