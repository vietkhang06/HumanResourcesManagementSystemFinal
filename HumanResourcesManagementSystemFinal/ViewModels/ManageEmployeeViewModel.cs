using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ManageEmployeeViewModel : ObservableObject
    {
        public ObservableCollection<Employee> Employees { get; set; } = new();
        public ObservableCollection<Department> Departments { get; set; } = new();

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private Department _selectedDepartment;

        private List<Employee> _allEmployees = new List<Employee>();

        public ManageEmployeeViewModel()
        {
            LoadDataFromDb();
        }

        public void LoadDataFromDb()
        {
            try
            {
                using (var context = new DataContext())
                {
                    var depts = context.Departments.ToList();
                    depts.Insert(0, new Department { DepartmentID = "", DepartmentName = "      --- Tất cả ---" });

                    Departments.Clear();
                    foreach (var d in depts) Departments.Add(d);

                    if (SelectedDepartment == null) SelectedDepartment = Departments.FirstOrDefault();

                    _allEmployees = context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.Position)
                        .Include(e => e.Manager)
                        .OrderByDescending(e => e.EmployeeID)
                        .ToList();

                    FilterEmployees();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        partial void OnSearchTextChanged(string value) => FilterEmployees();
        partial void OnSelectedDepartmentChanged(Department value) => FilterEmployees();

        private void FilterEmployees()
        {
            var query = _allEmployees.AsEnumerable();

            if (SelectedDepartment != null && !string.IsNullOrEmpty(SelectedDepartment.DepartmentID))
                query = query.Where(e => e.DepartmentID == SelectedDepartment.DepartmentID);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string key = SearchText.ToLower();
                query = query.Where(e =>
                    (e.FullName ?? "").ToLower().Contains(key) ||
                    (e.Email ?? "").ToLower().Contains(key) ||
                    (e.EmployeeID ?? "").ToLower().Contains(key));
            }

            Employees.Clear();
            foreach (var e in query) Employees.Add(e);
        }

        [RelayCommand]
        private void AddEmployee()
        {
            var addWindow = new AddEmployeeWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadDataFromDb();
            }
        }

        [RelayCommand]
        private void EditEmployee(Employee emp)
        {
            if (emp == null) return;

            // Mở cửa sổ ở chế độ SỬA (Truyền emp vào)
            var editWindow = new AddEmployeeWindow(emp);

            bool? result = editWindow.ShowDialog();

            // Nếu lưu thành công thì load lại danh sách
            if (result == true)
            {
                LoadDataFromDb();
            }
        }

        [RelayCommand]
        private async Task DeleteEmployee(Employee emp)
        {
            if (emp == null) return;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên {emp.FullName}?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new DataContext())
                    {
                        var dbEmp = context.Employees
                            .Include(e => e.Account)
                            .Include(e => e.WorkContracts)
                            .FirstOrDefault(e => e.EmployeeID == emp.EmployeeID);

                        if (dbEmp != null)
                        {
                            if (dbEmp.Account != null) context.Accounts.Remove(dbEmp.Account);
                            if (dbEmp.WorkContracts != null) context.WorkContracts.RemoveRange(dbEmp.WorkContracts);

                            var subordinates = context.Employees.Where(e => e.ManagerID == dbEmp.EmployeeID).ToList();
                            foreach (var sub in subordinates) sub.ManagerID = null;

                            context.Employees.Remove(dbEmp);
                            await context.SaveChangesAsync();

                            LoadDataFromDb();
                            MessageBox.Show("Đã xóa thành công!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi chi tiết: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void ViewDetail(Employee emp)
        {
            if (emp == null) return;
            // var detailWindow = new EmployeeDetailWindow(emp); // Bỏ comment khi bạn có cửa sổ này
            // detailWindow.ShowDialog();
        }
    }
}