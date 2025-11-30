using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models; // Để nhận Account từ Login
using HumanResourcesManagementSystemFinal.Views;  // Để new các UserControl (HomeControl...)
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = "Trang Chủ";
    [ObservableProperty]
    private object _currentView;
    public MainViewModel(Account currentAccount)
    {
        CurrentView = new HomeControl();
    }

    public MainViewModel()
    {
        CurrentView = new HomeControl();
    }


    [RelayCommand]
    private void NavigateHome()
    {
        PageTitle = "Trang Chủ";
        CurrentView = new HomeControl();
    }

    [RelayCommand]
    private void NavigateEmployee()
    {
        PageTitle = "Quản Lý Nhân Viên";
        CurrentView = new ManageEmployeeControl();
    }

    [RelayCommand]
    private void NavigateDepartment()
    {
        PageTitle = "Phòng Ban & Vị Trí";
        CurrentView = new Views.Department_Position_Control();
    }

    [RelayCommand]
    private void NavigatePayroll()
    {
        PageTitle = "Tính Lương";
        CurrentView = new PayrollControl();
    }

    [RelayCommand]
    private void NavigateProfile()
    {
        PageTitle = "Hồ Sơ Của Tôi";
        CurrentView = new ProfileControl();
    }

    [RelayCommand]
    private void NavigateTimeSheet()
    {
        PageTitle = "Chấm Công";
        CurrentView = new TimeSheetControl();
    }

    [RelayCommand]
    private void Logout(object parameter)
    {
        if (parameter is Window currentWindow)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            currentWindow.Close();
        }
    }
}