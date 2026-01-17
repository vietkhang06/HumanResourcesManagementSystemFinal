using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class EmployeeDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Employee _employee;

        public EmployeeDetailViewModel(Employee emp)
        {
            _employee = emp;
        }

        [RelayCommand]
        private void CloseWindow(Window window)
        {
            window?.Close();
        }
    }
}