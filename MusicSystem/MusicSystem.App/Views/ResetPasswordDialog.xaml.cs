using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for ResetPasswordDialog.xaml
    /// </summary>
    public partial class ResetPasswordDialog : Window
    {

        public string NewPassword { get; private set; }
        public ResetPasswordDialog(string username)
        {
            InitializeComponent();
            txtUsername.Text = $"Username: {username}";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewPassword.Password))
            {
                MessageBox.Show(
                    "Vui lòng nhập mật khẩu mới",
                    "Cảnh báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtNewPassword.Focus();
                return;
            }

            if (txtNewPassword.Password.Length < 6)
            {
                MessageBox.Show(
                    "Mật khẩu phải có ít nhất 6 ký tự",
                    "Cảnh báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtNewPassword.Focus();
                return;
            }

            NewPassword = txtNewPassword.Password;
            DialogResult = true;
            Close();
        }




    }
}
