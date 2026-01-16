using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class AddDepartmentViewModel : ObservableObject
    {
        [ObservableProperty] private string _title = "Thêm Phòng Ban Mới";
        [ObservableProperty] private string _deptName;
        [ObservableProperty] private string _deptLocation;
        [ObservableProperty] private string _selectedManagerID;

        // Danh sách nhân viên để hiển thị trong ComboBox
        public ObservableCollection<Employee> Employees { get; set; } = new();

        public AddDepartmentViewModel()
        {
            // Constructor mặc định dùng cho Thêm mới
            _ = LoadEmployeesAsync();
        }

        public AddDepartmentViewModel(Department existingDept)
        {
            // Constructor dùng cho Chỉnh sửa
            Title = "Cập Nhật Phòng Ban";
            DeptName = existingDept.DepartmentName;
            DeptLocation = existingDept.Location;
            SelectedManagerID = existingDept.ManagerID;

            _ = LoadEmployeesAsync();
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                using var context = new DataContext();
                // Chỉ lấy ID và Tên để nhẹ dữ liệu
                var list = await context.Employees
                    .Select(e => new Employee { EmployeeID = e.EmployeeID, FullName = e.FullName })
                    .ToListAsync();

                Employees.Clear();
                foreach (var emp in list) Employees.Add(emp);
            }
            catch { /* Xử lý lỗi nếu cần */ }
        }

        [RelayCommand]
        private void Save(Window window)
        {
            if (string.IsNullOrWhiteSpace(DeptName))
            {
                MessageBox.Show("Vui lòng nhập tên phòng ban!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Đóng cửa sổ và trả về True
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}