using System.Windows;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddNotificationWindow : Window
    {
        public AddNotificationWindow()
        {
            InitializeComponent();
        }
        public string NewNotificationTitle { get; set; }
        public string NewNotificationContent { get; set; } // Thêm cái này

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtNewTitle.Text))
            {
                NewNotificationTitle = TxtNewTitle.Text;
                NewNotificationContent = TxtNewContent.Text; // Lấy dữ liệu từ TextBox nội dung

                this.DialogResult = true;
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Thu nhỏ cửa sổ xuống Taskbar
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Phóng to hoặc thu nhỏ lại trạng thái cũ
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        // Đóng cửa sổ (Dùng lại hàm Cancel đã viết)
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // QUAN TRỌNG: Cho phép nắm kéo cửa sổ
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}