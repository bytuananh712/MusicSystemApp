using System.Windows;

namespace MusicSystem.App.Views
{
    public partial class RejectReasonDialog : Window
    {
        public string Reason { get; private set; }

        public RejectReasonDialog(string songTitle)
        {
            InitializeComponent();
            txtSongTitle.Text = $"Bài hát: {songTitle}";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập lý do từ chối",
                    "Cảnh báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtReason.Focus();
                return;
            }

            Reason = txtReason.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}