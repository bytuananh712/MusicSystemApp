using MusicSystem.App.Services;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Songs;
using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicSystem.App.Views
{
    public partial class EditSongWindow : Window
    {
        private readonly ISocketClient _socketClient;
        private readonly SongDto _song;

        public EditSongWindow(ISocketClient socketClient, SongDto song)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _song = song;

            LoadSongData();
        }

        private void LoadSongData()
        {
            txtSongTitle.Text = $"Bài hát: {_song.Title}";
            txtTitle.Text = _song.Title;
            txtReleaseYear.Text = _song.ReleaseYear?.ToString();
            txtLyrics.Text = _song.Lyrics;

            // Select genre
            foreach (ComboBoxItem item in cboGenre.Items)
            {
                if (item.Content.ToString() == _song.Genre)
                {
                    cboGenre.SelectedItem = item;
                    break;
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowError("Vui lòng nhập tiêu đề");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtReleaseYear.Text))
            {
                if (!int.TryParse(txtReleaseYear.Text.Trim(), out int year)
                    || year < 1900 || year > DateTime.Now.Year)
                {
                    ShowError($"Năm phát hành phải là số từ 1900 đến {DateTime.Now.Year}");
                    txtReleaseYear.Focus();
                    return;
                }
            }

            await UpdateSongAsync();
        }

        private async Task UpdateSongAsync()
        {
            try
            {
                btnSave.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                var updateDto = new UpdateSongDto
                {
                    Title = txtTitle.Text.Trim(),
                    Genre = (cboGenre.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    ReleaseYear = int.TryParse(txtReleaseYear.Text, out var year) ? year : null,
                    Lyrics = txtLyrics.Text.Trim()
                };

                var data = new Dictionary<string, object>
                {
                    { "songId", _song.SongId.ToString() },
                    { "data", updateDto }
                };

                var request = new SocketRequest
                {
                    Command = SocketCommands.UpdateSong,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = JsonSerializer.Serialize(data)
                };

                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    MessageBox.Show("Cập nhật thành công!", "Thành công");
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