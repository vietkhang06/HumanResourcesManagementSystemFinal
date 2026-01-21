using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class NotificationDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Notification _currentNotification;

        public NotificationDetailViewModel(Notification notification)
        {
            CurrentNotification = notification;
        }

        public NotificationDetailViewModel(){}

        [RelayCommand]
        private void CloseWindow(Window window)
        {
            window?.Close();
        }

        [RelayCommand]
        private void MinimizeWindow(Window window)
        {
            if (window != null)
                window.WindowState = WindowState.Minimized;
        }

        [RelayCommand]
        private void MaximizeWindow(Window window)
        {
            if (window != null)
            {
                if (window.WindowState == WindowState.Maximized) window.WindowState = WindowState.Normal;
                else window.WindowState = WindowState.Maximized;
            }
        }
    }
}