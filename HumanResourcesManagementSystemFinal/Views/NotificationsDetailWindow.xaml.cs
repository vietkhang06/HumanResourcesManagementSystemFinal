using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class NotificationsDetailWindow : Window
    {
        public NotificationsDetailWindow(Notification notification)
        {
            InitializeComponent();
            // Gán ViewModel và truyền dữ liệu vào
            DataContext = new NotificationDetailViewModel(notification);
        }

        // Hỗ trợ kéo thả cửa sổ khi không có thanh tiêu đề chuẩn
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}