using System.Windows;
using System.Windows.Input;
using HumanResourcesManagementSystemFinal.ViewModels;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Xử lý kéo thả cửa sổ (Drag Move)
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Xử lý nút thu nhỏ
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Xử lý nút đóng
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}