using MusicSystem.Shared.DTOs.Songs;
using System.Windows;

namespace MusicSystem.App.Views
{
    public partial class SongDetailWindow : Window
    {
        public SongDetailWindow(SongDto song)
        {
            InitializeComponent();

            txtTitle.Text = song.Title;
            txtArtists.Text = song.Artists ?? "Unknown";
            txtGenre.Text = song.Genre ?? "N/A";
            txtDuration.Text = song.DurationFormatted;
            txtFileSize.Text = song.FileSizeFormatted;
            txtFormat.Text = song.Format ?? "N/A";
            txtCreatedAt.Text = song.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
            txtFileUrl.Text = song.FileUrl;
            txtLyrics.Text = string.IsNullOrEmpty(song.Lyrics) ? "(Không có)" : song.Lyrics;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}