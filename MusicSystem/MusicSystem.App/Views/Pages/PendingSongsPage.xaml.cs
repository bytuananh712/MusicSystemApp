using MusicSystem.App.Services;
using MusicSystem.App.Views;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Songs;
using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicSystem.App.Views.Pages
{
    public partial class PendingSongsPage : Page
    {
        private readonly ISocketClient _socketClient;
        private List<SongDto> _pendingSongs;

        public PendingSongsPage(ISocketClient socketClient)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _pendingSongs = new List<SongDto>();

            Loaded += PendingSongsPage_Loaded;
        }

        private async void PendingSongsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPendingSongsAsync();
        }

        // ==================== LOAD DATA ====================
        private async Task LoadPendingSongsAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                btnRefresh.IsEnabled = false;

                var request = new SocketRequest
                {
                    Command = SocketCommands.GetPendingSongs,
                    Token = Application.Current.Properties["AuthToken"]?.ToString(),
                    Data = "{}"
                };

                var response = await _socketClient.SendRequestAsync(request);

                if (response.Status == SocketStatus.Success)
                {
                    _pendingSongs = JsonSerializer.Deserialize<List<SongDto>>(response.Data);

                    dgPendingSongs.ItemsSource = _pendingSongs;

                    if (_pendingSongs.Count == 0)
                    {
                        txtStatus.Text = "✅ Không có bài hát chờ duyệt";
                    }
                    else
                    {
                        txtStatus.Text = $"⏳ Có {_pendingSongs.Count} bài hát chờ duyệt";
                    }
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
                    $"Không thể tải danh sách:\n{ex.Message}",
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

        // ==================== REFRESH ====================
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadPendingSongsAsync();
        }

        // ==================== VIEW DETAILS ====================
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid songId)
            {
                var song = _pendingSongs.FirstOrDefault(s => s.SongId == songId);
                if (song == null) return;

                var detailWindow = new SongDetailWindow(song);
                detailWindow.Owner = Window.GetWindow(this);
                detailWindow.ShowDialog();
            }
        }

        // ==================== APPROVE ====================
        private async void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid songId)
            {
                var song = _pendingSongs.FirstOrDefault(s => s.SongId == songId);
                if (song == null) return;

                var confirm = MessageBox.Show(
                    $"Bạn có chắc muốn PHÊ DUYỆT bài hát này?\n\n" +
                    $"📌 Tiêu đề: {song.Title}\n" +
                    $"🎤 Nghệ sĩ: {song.Artists}\n" +
                    $"🎵 Thể loại: {song.Genre}\n\n" +
                    $"Sau khi duyệt, bài hát sẽ hiển thị công khai cho người dùng.",
                    "Xác nhận phê duyệt",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;

                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    var request = new SocketRequest
                    {
                        Command = SocketCommands.ApproveSong,
                        Token = Application.Current.Properties["AuthToken"]?.ToString(),
                        Data = songId.ToString()
                    };

                    var response = await _socketClient.SendRequestAsync(request);

                    if (response.Status == SocketStatus.Success)
                    {
                        MessageBox.Show(
                            $"Đã phê duyệt bài hát '{song.Title}'\n\n" +
                            $"Bài hát đã được công khai và người dùng có thể nghe.",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadPendingSongsAsync();
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

        // ==================== REJECT ====================
        private async void btnReject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid songId)
            {
                var song = _pendingSongs.FirstOrDefault(s => s.SongId == songId);
                if (song == null) return;

                var reasonDialog = new RejectReasonDialog(song.Title);
                reasonDialog.Owner = Window.GetWindow(this);

                if (reasonDialog.ShowDialog() != true)
                    return;

                var reason = reasonDialog.Reason;

                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    var data = new Dictionary<string, string>
                    {
                        { "songId", songId.ToString() },
                        { "reason", reason }
                    };

                    var request = new SocketRequest
                    {
                        Command = SocketCommands.RejectSong,
                        Token = Application.Current.Properties["AuthToken"]?.ToString(),
                        Data = JsonSerializer.Serialize(data)
                    };

                    var response = await _socketClient.SendRequestAsync(request);

                    if (response.Status == SocketStatus.Success)
                    {
                        MessageBox.Show(
                            $"❌ Đã từ chối bài hát '{song.Title}'\n\n" +
                            $"Lý do: {reason}",
                            "Đã từ chối",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadPendingSongsAsync();
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