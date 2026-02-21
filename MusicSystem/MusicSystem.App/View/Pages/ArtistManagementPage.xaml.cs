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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicSystem.App.Views.Pages
{
    /// <summary>
    /// Interaction logic for ArtistManagementPage.xaml
    /// </summary>
    public partial class ArtistManagementPage : Page
    {

        private readonly ISocketClient _socketClient;
        private List<ArtistDto> _allArtists;
        private List<ArtistDto> _filteredArtists;
        public ArtistManagementPage(ISocketClient socketClient)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _allArtists = new List<ArtistDto>();
            _filteredArtists = new List<ArtistDto>();

            // Load data khi page load
            Loaded += ArtistManagementPage_Loaded;
        }


        private async void ArtistManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadArtistsAsync();
        }

        //LOAD DATA 
        private async Task LoadArtistsAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                btnRefresh.IsEnabled = false;

                // Tạo request
                var request = new SocketRequest
                {
                    Command = SocketCommands.GetAllArtists,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = "{}"
                };

                // Gửi request
                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    // Parse response
                    _allArtists = JsonSerializer.Deserialize<List<ArtistDto>>(response.Data);
                    _filteredArtists = _allArtists.ToList();

                    // Bind to DataGrid
                    dgArtists.ItemsSource = _filteredArtists;

                    // Update status
                    txtStatus.Text = $"Tổng: {_allArtists.Count} nghệ sĩ";
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
                    $"Không thể tải danh sách nghệ sĩ:\n{ex.Message}",
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

        //  SEARCH
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var searchTerm = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                _filteredArtists = _allArtists.ToList();
            }
            else
            {
                _filteredArtists = _allArtists.Where(a =>
                    a.ArtistName.ToLower().Contains(searchTerm) ||
                    (a.Biography != null && a.Biography.ToLower().Contains(searchTerm))
                ).ToList();
            }

            dgArtists.ItemsSource = null;
            dgArtists.ItemsSource = _filteredArtists;
            txtStatus.Text = $"Tìm thấy: {_filteredArtists.Count}/{_allArtists.Count} nghệ sĩ";
        }

        //  REFRESH 
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            await LoadArtistsAsync();
        }

        //  ADD ARTIST 
        private async void btnAddArtist_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddArtistWindow(_socketClient);
            addWindow.Owner = Window.GetWindow(this);

            var result = addWindow.ShowDialog();

            if (result == true)
            {
                await LoadArtistsAsync();
            }
        }

        //  EDIT ARTIST 
        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid artistId)
            {
                var artist = _allArtists.FirstOrDefault(a => a.ArtistId == artistId);
                if (artist == null) return;

                var editWindow = new EditArtistWindow(_socketClient, artist);
                editWindow.Owner = Window.GetWindow(this);

                var result = editWindow.ShowDialog();

                if (result == true)
                {
                    await LoadArtistsAsync();
                }
            }
        }

        // DELETE ARTIST 
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid artistId)
            {
                var artist = _allArtists.FirstOrDefault(a => a.ArtistId == artistId);
                if (artist == null) return;

                var confirm = MessageBox.Show(
                    $"Bạn có chắc muốn xóa nghệ sĩ '{artist.ArtistName}'?\n\n" +
                    $"Lưu ý: Nếu nghệ sĩ có bài hát, bạn nên Ẩn thay vì Xóa.",
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
                        Command = SocketCommands.DeleteArtist,
                        Token = Application.Current.Properties["AuthToken"]?.ToString(),
                        Data = artistId.ToString()
                    };

                    var response = await _socketClient.SendRequestAsync(request);

                    if (response.Status == SocketStatus.Success)
                    {
                        MessageBox.Show(
                            $"Đã xóa nghệ sĩ '{artist.ArtistName}'",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadArtistsAsync();
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

        //  DISABLE ARTIST 
        private async void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid artistId)
            {
                var artist = _allArtists.FirstOrDefault(a => a.ArtistId == artistId);
                if (artist == null) return;

                var confirm = MessageBox.Show(
                    $"Bạn có chắc muốn ẩn nghệ sĩ '{artist.ArtistName}'?\n\n" +
                    $"Nghệ sĩ này sẽ không hiển thị trên trang web.",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;

                try
                {
                    var request = new SocketRequest
                    {
                        Command = SocketCommands.DisableArtist,
                        Token = Application.Current.Properties["AuthToken"]?.ToString(),
                        Data = artistId.ToString()
                    };

                    var response = await _socketClient.SendRequestAsync(request);

                    if (response.Status == SocketStatus.Success)
                    {
                        MessageBox.Show(
                            $"Đã ẩn nghệ sĩ '{artist.ArtistName}'",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadArtistsAsync();
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

        //  ENABLE ARTIST 
        private async void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid artistId)
            {
                var artist = _allArtists.FirstOrDefault(a => a.ArtistId == artistId);
                if (artist == null) return;

                try
                {
                    var request = new SocketRequest
                    {
                        Command = SocketCommands.EnableArtist,
                        Token = Application.Current.Properties["AuthToken"]?.ToString(),
                        Data = artistId.ToString()
                    };

                    var response = await _socketClient.SendRequestAsync(request);

                    if (response.Status == SocketStatus.Success)
                    {
                        MessageBox.Show(
                            $"Đã hiển thị nghệ sĩ '{artist.ArtistName}'",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadArtistsAsync();
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
