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
        private List<Employee> _allEmployees = new();
        public ObservableCollection<Employee> Employees { get; } = new();
        public ObservableCollection<Department> Departments { get; } = new();

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private Department _selectedDepartment;

        public ManageEmployeeViewModel()
        {
            LoadDataFromDb();
        }

        public void LoadDataFromDb()
        {
            try
            {
                using var context = new DataContext();

                var deptList = context.Departments.ToList();
                deptList.Insert(0, new Department
                {
                    DepartmentID = "",
                    DepartmentName = "--- Tất cả ---"
                });

                Departments.Clear();
                foreach (var dept in deptList)
                    Departments.Add(dept);

                SelectedDepartment ??= Departments.FirstOrDefault();

                _allEmployees = context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.Manager)
                    .OrderByDescending(e => e.EmployeeID)
                    .ToList();

                FilterEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        partial void OnSearchTextChanged(string value) => FilterEmployees();
        partial void OnSelectedDepartmentChanged(Department value) => FilterEmployees();

        private void FilterEmployees()
        {
            IEnumerable<Employee> query = _allEmployees;

            if (SelectedDepartment != null && !string.IsNullOrEmpty(SelectedDepartment.DepartmentID))
            {
                query = query.Where(e => e.DepartmentID == SelectedDepartment.DepartmentID);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.ToLower();
                query = query.Where(e =>
                    (e.FullName ?? "").ToLower().Contains(keyword) ||
                    (e.Email ?? "").ToLower().Contains(keyword) ||
                    (e.EmployeeID ?? "").ToLower().Contains(keyword));
            }

            Employees.Clear();
            foreach (var emp in query)
                Employees.Add(emp);
        }

        [RelayCommand]
        private void AddEmployee()
        {
            var window = new AddEmployeeWindow();
            if (window.ShowDialog() == true)
                LoadDataFromDb();
        }

        [RelayCommand]
        private void EditEmployee(Employee emp)
        {
            if (emp == null) return;

            var window = new AddEmployeeWindow(emp);
            if (window.ShowDialog() == true)
                LoadDataFromDb();
        }

        [RelayCommand]
        private async Task DeleteEmployee(Employee emp)
        {
            if (emp == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nhân viên {emp.FullName}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var context = new DataContext();

                var dbEmp = await context.Employees
                    .Include(e => e.Account)
                    .Include(e => e.WorkContracts)
                    .FirstOrDefaultAsync(e => e.EmployeeID == emp.EmployeeID);

                if (dbEmp == null) return;

                if (dbEmp.Account != null)
                    context.Accounts.Remove(dbEmp.Account);

                if (dbEmp.WorkContracts != null)
                    context.WorkContracts.RemoveRange(dbEmp.WorkContracts);

                var subordinates = context.Employees
                    .Where(e => e.ManagerID == dbEmp.EmployeeID)
                    .ToList();

                foreach (var sub in subordinates)
                    sub.ManagerID = null;

                string adminID = string.IsNullOrEmpty(UserSession.CurrentEmployeeId)
                    ? "ADMIN"
                    : UserSession.CurrentEmployeeId;

                context.ChangeHistories.Add(new ChangeHistory
                {
                    LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    TableName = "Employees",
                    ActionType = "DELETE",
                    RecordID = dbEmp.EmployeeID,
                    ChangeByUserID = adminID,
                    ChangeTime = DateTime.Now,
                    Details = $"Xóa nhân viên: {dbEmp.FullName} (ID: {dbEmp.EmployeeID})"
                });

                context.Employees.Remove(dbEmp);
                await context.SaveChangesAsync();

                LoadDataFromDb();
                MessageBox.Show("Đã xóa thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        [RelayCommand]
        private void ViewDetail(Employee emp)
        {
            if (emp == null) return;
        }
    }
}
