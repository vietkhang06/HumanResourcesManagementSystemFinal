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
        private void UserProfile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button; 
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top; 
                button.ContextMenu.IsOpen = true;
            }
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
            this.DataContext = new MainViewModel(user);
        }
    }
}