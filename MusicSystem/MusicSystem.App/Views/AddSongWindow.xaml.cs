using Microsoft.Win32;
using MusicSystem.App.Services;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Artists;
using MusicSystem.Shared.DTOs.Songs;
using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MusicSystem.App.Views
{
    public partial class AddSongWindow : Window
    {
        private readonly ISocketClient _socketClient;
        private string _selectedFilePath;
        private List<ArtistDto> _allArtists;
        private List<Guid> _selectedArtistIds;
        private UploadFileResponse _uploadedFile;

        public AddSongWindow(ISocketClient socketClient)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _allArtists = new List<ArtistDto>();
            _selectedArtistIds = new List<Guid>();

            // Load artists
            Loaded += AddSongWindow_Loaded;
        }

        private async void AddSongWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadArtistsAsync();
        }

        // ==================== LOAD ARTISTS ====================
        private async Task LoadArtistsAsync()
        {
            try
            {
                var request = new SocketRequest
                {
                    Command = SocketCommands.GetAllArtists,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = "{}"
                };

                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    _allArtists = JsonSerializer.Deserialize<List<ArtistDto>>(response.Data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách nghệ sĩ: {ex.Message}", "Lỗi");
            }
        }

        // ==================== SELECT FILE ====================
        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Chọn file nhạc",
                Filter = "Audio Files|*.mp3;*.flac;*.wav;*.m4a|MP3 Files|*.mp3|FLAC Files|*.flac|All Files|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;

                // Show file info
                var fileInfo = new FileInfo(_selectedFilePath);
                txtFileName.Text = $"File: {fileInfo.Name}";
                txtFileSize.Text = $"Size: {fileInfo.Length / 1024.0 / 1024.0:F2} MB";
                fileInfoPanel.Visibility = Visibility.Visible;

                // Auto-fill title from filename (without extension)
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    txtTitle.Text = Path.GetFileNameWithoutExtension(fileInfo.Name);
                }
            }
        }

        // ==================== SELECT ARTISTS ====================
        private void btnSelectArtists_Click(object sender, RoutedEventArgs e)
        {
            var selectWindow = new SelectArtistsDialog(_allArtists, _selectedArtistIds);
            selectWindow.Owner = this;

            if (selectWindow.ShowDialog() == true)
            {
                _selectedArtistIds = selectWindow.SelectedArtistIds;

                // Update display
                lstSelectedArtists.Items.Clear();
                foreach (var artistId in _selectedArtistIds)
                {
                    var artist = _allArtists.FirstOrDefault(a => a.ArtistId == artistId);
                    if (artist != null)
                    {
                        lstSelectedArtists.Items.Add(artist.ArtistName);
                    }
                }
            }
        }

        // ==================== CANCEL ====================
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ==================== SAVE ====================
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                ShowError("Vui lòng chọn file nhạc");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowError("Vui lòng nhập tiêu đề bài hát");
                txtTitle.Focus();
                return;
            }

            if (_selectedArtistIds.Count == 0)
            {
                ShowError("Vui lòng chọn ít nhất 1 nghệ sĩ");
                return;
            }

            // Upload file first
            await UploadAndSaveAsync();
        }

        // ==================== UPLOAD & SAVE ====================
        private async Task UploadAndSaveAsync()
        {
            try
            {
                btnSave.IsEnabled = false;
                btnCancel.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                // Step 1: Upload file
                progressPanel.Visibility = Visibility.Visible;
                txtProgress.Text = "Đang upload file...";
                progressBar.Value = 30;

                var fileBytes = await File.ReadAllBytesAsync(_selectedFilePath);
                var uploadRequest = new UploadFileRequest
                {
                    FileName = Path.GetFileName(_selectedFilePath),
                    FileData = fileBytes,
                    ContentType = "audio/mpeg"
                };

                var uploadSocketRequest = new SocketRequest
                {
                    Command = SocketCommands.UploadSongFile,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = JsonSerializer.Serialize(uploadRequest)
                };

                var uploadResponse = await _socketClient.SendRequestAsync(uploadSocketRequest);

                if (uploadResponse.Status != SocketStatus.Success)
                {
                    ShowError($"Upload thất bại: {uploadResponse.Message}");
                    return;
                }

                _uploadedFile = JsonSerializer.Deserialize<UploadFileResponse>(uploadResponse.Data);

                if (!_uploadedFile.Success)
                {
                    ShowError($"Upload thất bại: {_uploadedFile.Message}");
                    return;
                }

                progressBar.Value = 60;

                // Step 2: Create song
                txtProgress.Text = "Đang lưu thông tin bài hát...";

                var createDto = new CreateSongDto
                {
                    Title = txtTitle.Text.Trim(),
                    FileUrl = _uploadedFile.FileUrl,
                    FileSize = _uploadedFile.FileSize,
                    Duration = _uploadedFile.Duration,
                    Format = _uploadedFile.Format,
                    Genre = (cboGenre.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString(),
                    ReleaseYear = int.TryParse(txtReleaseYear.Text, out var year) ? year : null,
                    Lyrics = txtLyrics.Text.Trim(),
                    ArtistIds = _selectedArtistIds
                };

                var createRequest = new SocketRequest
                {
                    Command = SocketCommands.CreateSong,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = JsonSerializer.Serialize(createDto)
                };

                var createResponse = await _socketClient.SendRequestAsync(createRequest);

                progressBar.Value = 100;

                if (createResponse.Status == SocketStatus.Success)
                {
                    MessageBox.Show(
                        "Thêm bài hát thành công!\n\nBài hát đang ở trạng thái 'Pending', cần Manager duyệt.",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError(createResponse.Message);
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
                progressPanel.Visibility = Visibility.Collapsed;
                progressBar.Value = 0;
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }
    }
}