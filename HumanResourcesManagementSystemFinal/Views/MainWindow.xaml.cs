using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void UserProfile_Click(object sender, RoutedEventArgs e)
        {
            // Mở ContextMenu khi nhấn vào vùng Avatar/User
            if (BtnUserProfile.ContextMenu != null)
            {
                BtnUserProfile.ContextMenu.PlacementTarget = BtnUserProfile;
                BtnUserProfile.ContextMenu.IsOpen = true;
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        // --- CÁC NÚT ĐIỀU KHIỂN CỬA SỔ ---
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                iconMaximize.Data = Geometry.Parse("M6,8 V4 H20 V18 H16 M6,8 H16 V18 H6 Z");
            }
            else
            {
                this.WindowState = WindowState.Normal;
                iconMaximize.Data = Geometry.Parse("M4,4 H20 V20 H4 Z M8,8 V16 H16 V8 Z");
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Sidebar_MouseEnter(object sender, MouseEventArgs e)
        {
            var sb = this.FindResource("OpenMenuAnimation") as Storyboard;
            sb?.Begin();
        }
        private void Sidebar_MouseLeave(object sender, MouseEventArgs e)
        {
            // Nếu ContextMenu của User đang mở thì KHÔNG ĐƯỢC ĐÓNG Sidebar
            if (UserContextMenu.IsOpen) return;

            var sb = this.FindResource("CloseMenuAnimation") as Storyboard;
            sb?.Begin();
        }
        private void UserContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem chuột còn nằm trong vùng Sidebar không?
            // Nếu chuột đã chạy ra ngoài rồi thì đóng Sidebar lại.
            if (!SidebarBorder.IsMouseOver)
            {
                var sb = this.FindResource("CloseMenuAnimation") as Storyboard;
                sb?.Begin();
            }
        }
    }
}