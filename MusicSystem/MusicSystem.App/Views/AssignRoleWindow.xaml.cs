using MusicSystem.Shared.DTOs.Users;
using System.Collections.Generic;
using System.Windows;

namespace MusicSystem.App.Views
{
    /// <summary>
    /// View-only window showing the current roles of a user.
    /// </summary>
    public partial class AssignRoleWindow : Window
    {
        private readonly UserManagementDto _user;

        public AssignRoleWindow(object socketClient, UserManagementDto user)
        {
            InitializeComponent();
            _user = user;
            LoadUserRoles();
        }

        private void LoadUserRoles()
        {
            txtUserInfo.Text = $"Username: {_user.Username} | Email: {_user.Email}";

            if (_user.Roles != null && _user.Roles.Count > 0)
            {
                lstRoles.ItemsSource = _user.Roles;
                txtNoRole.Visibility = Visibility.Collapsed;
            }
            else
            {
                lstRoles.ItemsSource = null;
                txtNoRole.Visibility = Visibility.Visible;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
