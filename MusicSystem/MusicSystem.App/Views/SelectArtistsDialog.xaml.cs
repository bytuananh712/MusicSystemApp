using MusicSystem.Shared.DTOs.Artists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MusicSystem.App.Views
{
    public partial class SelectArtistsDialog : Window
    {
        public List<Guid> SelectedArtistIds { get; private set; }

        public SelectArtistsDialog(List<ArtistDto> allArtists, List<Guid> currentSelectedIds)
        {
            InitializeComponent();

            // Bind artists
            lstArtists.ItemsSource = allArtists;

            // Select current artists
            foreach (var artist in allArtists)
            {
                if (currentSelectedIds.Contains(artist.ArtistId))
                {
                    lstArtists.SelectedItems.Add(artist);
                }
            }

            SelectedArtistIds = new List<Guid>(currentSelectedIds);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (lstArtists.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn ít nhất 1 nghệ sĩ",
                    "Cảnh báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SelectedArtistIds = lstArtists.SelectedItems
                .Cast<ArtistDto>()
                .Select(a => a.ArtistId)
                .ToList();

            DialogResult = true;
            Close();
        }
    }
}