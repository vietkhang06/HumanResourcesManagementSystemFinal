using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
        public MainWindow(Employee user)
        {
            InitializeComponent();
            if (user?.Account == null)
                throw new ArgumentException("Employee must have an associated Account.", nameof(user));
            this.DataContext = new MainViewModel(user.Account);
        }
        private void UserProfile_Click(object sender, RoutedEventArgs e)
        {
            // Lấy đối tượng nút bấm
            var btn = sender as Button;

            // Nếu nút có chứa ContextMenu
            if (btn != null && btn.ContextMenu != null)
            {
                // Đảm bảo Menu biết nó mọc ra từ nút nào
                btn.ContextMenu.PlacementTarget = btn;

                // Mở Menu
                btn.ContextMenu.IsOpen = true;
            }
        }
    }
}