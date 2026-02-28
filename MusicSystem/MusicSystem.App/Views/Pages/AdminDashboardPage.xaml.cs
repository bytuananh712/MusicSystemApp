using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicSystem.App.Services;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Auth;
using MusicSystem.Shared.DTOs.Songs;
using MusicSystem.Shared.SocketContracts;

namespace MusicSystem.App.Views.Pages
{
    public partial class AdminDashboardPage : Page
    {
        private readonly ISocketClient _socketClient;
        private readonly UserDto _currentUser;
        private List<SongDto> _pendingSongs;

        public AdminDashboardPage(ISocketClient socketClient, UserDto currentUser)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _currentUser = currentUser;
            _pendingSongs = new List<SongDto>();

            txtWelcome.Text = $"Xin chào, {_currentUser.FullName ?? _currentUser.Username}!";

            if (_currentUser != null && _currentUser.Roles.Contains("Manager") && !_currentUser.Roles.Contains("Admin"))
            {
                if (FindName("txtDashboardTitle") is TextBlock txtTitle)
                {
                    txtTitle.Text = "🎵 Manager Dashboard";
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadStatisticsAsync();
            await LoadPendingSongsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                txtStatus.Text = "Đang tải dữ liệu...";
                Mouse.OverrideCursor = Cursors.Wait;

                // Show/hide UI elements based on role
                bool isAdmin = _currentUser.Roles.Contains("Admin");
                bool isManager = _currentUser.Roles.Contains("Manager");

                // Thống kê Users & Quyền nâng cao (Chỉ Admin)
                if (isAdmin)
                {
                    if (FindName("btnManageUsers") is Button btnManage) btnManage.Visibility = Visibility.Visible;
                    if (FindName("tabSongApproval") is TabItem tabApprove) tabApprove.Visibility = Visibility.Visible;

                    var reqUsers = new SocketRequest { Command = SocketCommands.GetAllUsers, Token = GetToken(), Data = "{}" };
                    var resUsers = await _socketClient.SendRequestAsync(reqUsers);
                    if (resUsers.Status == SocketStatus.Success)
                    {
                        var users = JsonSerializer.Deserialize<List<UserDto>>(resUsers.Data);
                        txtTotalUsers.Text = users?.Count.ToString() ?? "0";
                        txtActiveUsers.Text = users?.Count.ToString() ?? "0";
                    }
                }
                else
                {
                    if (FindName("btnManageUsers") is Button btnManage) btnManage.Visibility = Visibility.Collapsed;
                    if (FindName("tabSongApproval") is TabItem tabApprove) tabApprove.Visibility = Visibility.Collapsed;

                    txtTotalUsers.Text = "N/A";
                    txtActiveUsers.Text = "N/A";
                }

                // Thống kê bài hát & nghệ sĩ (Cho cả Admin và Manager)
                if (isAdmin || isManager)
                {
                    var reqSongs = new SocketRequest { Command = SocketCommands.GetAllSongs, Token = GetToken(), Data = "{}" };
                    var resSongs = await _socketClient.SendRequestAsync(reqSongs);
                    if (resSongs.Status == SocketStatus.Success)
                    {
                        var songs = JsonSerializer.Deserialize<List<SongDto>>(resSongs.Data);
                        txtTotalSongs.Text = songs?.Count.ToString() ?? "0";
                    }

                    var reqArtists = new SocketRequest { Command = SocketCommands.GetAllArtists, Token = GetToken(), Data = "{}" };
                    var resArtists = await _socketClient.SendRequestAsync(reqArtists);
                    if (resArtists.Status == SocketStatus.Success)
                    {
                        var artists = JsonSerializer.Deserialize<List<object>>(resArtists.Data);
                        txtTotalArtists.Text = artists?.Count.ToString() ?? "0";
                    }
                }

                txtStatus.Text = "Sẵn sàng";
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Lỗi tải thống kê: " + ex.Message;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task LoadPendingSongsAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var request = new SocketRequest
                {
                    Command = SocketCommands.GetPendingSongs,
                    Token = GetToken(),
                    Data = "{}"
                };

                var response = await _socketClient.SendRequestAsync(request);
                if (response.Status == SocketStatus.Success)
                {
                    _pendingSongs = JsonSerializer.Deserialize<List<SongDto>>(response.Data) ?? new List<SongDto>();
                    dgPendingSongs.ItemsSource = _pendingSongs;
                    txtPendingCount.Text = _pendingSongs.Count > 0 ? $"({_pendingSongs.Count})" : "";
                }
            }
            catch { }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void btnUserMgmt_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.Roles.Contains("Admin"))
            {
                MessageBox.Show("Tính năng này chỉ dành cho Admin.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Trả về view qua MainWindow
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.btnUsers_Click(null, null);
            }
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadStatisticsAsync();
        }

        private async void btnLoadPending_Click(object sender, RoutedEventArgs e)
        {
            await LoadPendingSongsAsync();
        }

        private void dgPendingSongs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = dgPendingSongs.SelectedItem as SongDto;
            btnApproveSong.IsEnabled = selected != null;
            btnRejectSong.IsEnabled = selected != null;
        }

        private async void btnApproveSong_Click(object sender, RoutedEventArgs e)
        {
            if (dgPendingSongs.SelectedItem is SongDto song)
            {
                var confirm = MessageBox.Show($"Duyệt bài hát '{song.Title}' của {song.Artists}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                var request = new SocketRequest
                {
                    Command = SocketCommands.ApproveSong,
                    Token = GetToken(),
                    Data = song.SongId.ToString()
                };
                var response = await _socketClient.SendRequestAsync(request);
                if (response.Status == SocketStatus.Success)
                {
                    MessageBox.Show("Đã duyệt bài: " + song.Title, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadPendingSongsAsync();
                    await LoadStatisticsAsync();
                }
                else
                {
                    MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnRejectSong_Click(object sender, RoutedEventArgs e)
        {
            if (dgPendingSongs.SelectedItem is SongDto song)
            {
                var reasonDialog = new RejectReasonDialog(song.Title);
                reasonDialog.Owner = Window.GetWindow(this);

                if (reasonDialog.ShowDialog() != true) return;
                var reason = reasonDialog.Reason;

                var data = new Dictionary<string, string>
                {
                    { "songId", song.SongId.ToString() },
                    { "reason", reason }
                };

                var request = new SocketRequest
                {
                    Command = SocketCommands.RejectSong,
                    Token = GetToken(),
                    Data = JsonSerializer.Serialize(data)
                };

                var response = await _socketClient.SendRequestAsync(request);
                if (response.Status == SocketStatus.Success)
                {
                    MessageBox.Show("Đã từ chối bài: " + song.Title, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadPendingSongsAsync();
                    await LoadStatisticsAsync();
                }
                else
                {
                    MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GetToken() => Application.Current.Properties["AuthToken"]?.ToString() ?? "";
    }
}
