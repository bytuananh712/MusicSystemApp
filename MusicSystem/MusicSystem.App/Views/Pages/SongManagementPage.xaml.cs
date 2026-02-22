using MusicSystem.App.Services;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Songs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicSystem.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for SongManagementPage.xaml
    /// </summary>
    public partial class SongManagementPage : Page
    {
        private readonly ISocketClient _socketClient;
        private List<SongDto> _allSongs;
        private List<SongDto> _filteredSongs;

        public SongManagementPage(ISocketClient socketClient)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _allSongs = new List<SongDto>();
            _filteredSongs = new List<SongDto>();

            Loaded += SongManagementPage_Loaded;
        }

        private async void SongManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSongsAsync();
        }

        // LOAD DATA 
        private async Task LoadSongsAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                btnRefresh.IsEnabled = false;

                var request = new SocketRequest
                {
                    Command = SocketCommands.GetAllSongs,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = "{}"
                };

                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    _allSongs = JsonSerializer.Deserialize<List<SongDto>>(response.Data);
                    _filteredSongs = _allSongs.ToList();

                    dgSongs.ItemsSource = _filteredSongs;
                    txtStatus.Text = $"Tổng: {_allSongs.Count} bài hát";
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
                    $"Không thể tải danh sách bài hát:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                btnRefresh.IsEnabled = true;
            }
        }

        // SEARCH
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var searchTerm = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                _filteredSongs = _allSongs.ToList();
            }
            else
            {
                _filteredSongs = _allSongs.Where(s =>
                    s.Title.ToLower().Contains(searchTerm) ||
                    (s.Artists != null && s.Artists.ToLower().Contains(searchTerm)) ||
                    (s.Genre != null && s.Genre.ToLower().Contains(searchTerm))
                ).ToList();
            }

            dgSongs.ItemsSource = null;
            dgSongs.ItemsSource = _filteredSongs;
            txtStatus.Text = $"Tìm thấy: {_filteredSongs.Count}/{_allSongs.Count} bài hát";
        }

        //  REFRESH 
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            await LoadSongsAsync();
        }

        //  ADD SONG 
        private async void btnAddSong_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddSongWindow(_socketClient);
            addWindow.Owner = Window.GetWindow(this);

            var result = addWindow.ShowDialog();

            if (result == true)
            {
                await LoadSongsAsync();
            }
        }

        // EDIT SONG 
        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid songId)
            {
                var song = _allSongs.FirstOrDefault(s => s.SongId == songId);
                if (song == null) return;

                var editWindow = new EditSongWindow(_socketClient, song);
                editWindow.Owner = Window.GetWindow(this);

                var result = editWindow.ShowDialog();

                if (result == true)
                {
                    await LoadSongsAsync();
                }
            }
        }

        //  DELETE SONG
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid songId)
            {
                var song = _allSongs.FirstOrDefault(s => s.SongId == songId);
                if (song == null) return;

                var confirm = MessageBox.Show(
                    $"Bạn có chắc muốn xóa bài hát '{song.Title}'?\n\n" +
                    $"⚠ Cảnh báo: File nhạc sẽ bị xóa vĩnh viễn!",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes)
                    return;

                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    var request = new SocketRequest
                    {
                        Command = SocketCommands.DeleteSong,
                        Token = Application.Current.Properties["AuthToken"]?.ToString(),
                        Data = songId.ToString()
                    };

                    var response = await _socketClient.SendRequestAsync(request);

                    if (response.Status == SocketStatus.Success)
                    {
                        MessageBox.Show(
                            $"Đã xóa bài hát '{song.Title}'",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadSongsAsync();
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
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}
