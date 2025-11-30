using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ManageEmployeeViewModel : ObservableObject
    {
        public ObservableCollection<Employee> Employees { get; set; } = new();

        public ObservableCollection<Department> Departments { get; set; } = new();

        [ObservableProperty]
        private string _searchText;

        public ManageEmployeeViewModel()
        {
            LoadData();
        }

        private void LoadData()
        {
            Employees.Add(new Employee { Id = 1, FirstName = "Nguyễn", LastName = "Văn A", Email = "a@gmail.com", PhoneNumber = "0909123456" });
        }

        [RelayCommand]
        private void AddEmployee() { /* Logic mở form thêm */ }

        [RelayCommand]
        private void EditEmployee(Employee emp) { /* Logic sửa */ }

        [RelayCommand]
        private void DeleteEmployee(Employee emp) { /* Logic xóa */ }
    }
}
