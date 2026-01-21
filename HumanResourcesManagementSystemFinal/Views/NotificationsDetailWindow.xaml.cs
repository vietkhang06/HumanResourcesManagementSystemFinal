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
            DataContext = new NotificationDetailViewModel(notification);
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}