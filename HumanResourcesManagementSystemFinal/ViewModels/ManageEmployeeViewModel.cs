using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
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
                    .AsNoTracking()
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.Manager)
                    .OrderByDescending(e => e.EmployeeID)
                    .ToList();

                FilterEmployees();
            }
            catch (Exception ex)
            {
                var errorMsg = $"Lỗi kết nối CSDL: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $"\nChi tiết: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMsg, "Lỗi Tải Dữ Liệu", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private void ShowAddEmployee(Employee existingEmp = null)
        {
            var addVM = existingEmp != null
                ? new AddEmployeeViewModel(existingEmp)
                : new AddEmployeeViewModel();

            var window = new AddEmployeeWindow { DataContext = addVM };

            if (window.ShowDialog() == true)
            {
                LoadDataFromDb();

                if (addVM.IsEditMode && addVM.EditingEmployeeId == AppSession.CurrentUser?.EmployeeID)
                {
                    if (Application.Current.MainWindow.DataContext is MainViewModel mainVM)
                    {
                        mainVM.RefreshCurrentUser();
                    }
                }
            }
        }

        [RelayCommand]
        private void AddEmployee() => ShowAddEmployee(null);

        [RelayCommand]
        private void EditEmployee(Employee emp)
        {
            if (emp != null) ShowAddEmployee(emp);
        }

        [RelayCommand]
        private async Task DeleteEmployee(Employee emp)
        {
            if (emp == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nhân viên {emp.FullName}?\nHành động này sẽ xóa cả tài khoản và hợp đồng liên quan.",
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

                if (dbEmp == null)
                {
                    MessageBox.Show("Nhân viên này không còn tồn tại hoặc đã bị xóa trước đó.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDataFromDb();
                    return;
                }

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
                MessageBox.Show("Đã xóa nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbUpdateException dbEx)
            {
                var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Không thể xóa nhân viên này do ràng buộc dữ liệu.\nChi tiết kỹ thuật: {innerMsg}", "Lỗi CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi không mong muốn:\n{ex.Message}\n\nStack Trace: {ex.StackTrace}", "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ViewDetail(Employee emp)
        {
            if (emp == null) return;
        }
    }
}