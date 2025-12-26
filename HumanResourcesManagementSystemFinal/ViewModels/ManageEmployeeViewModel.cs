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
                    // Tạo item "Tất cả" với ID rỗng hoặc đặc biệt
                    depts.Insert(0, new Department { DepartmentID = "", DepartmentName = "      --- Tất cả ---" });

                    Departments.Clear();
                    foreach (var d in depts) Departments.Add(d);

                    SelectedDepartment = Departments.FirstOrDefault();

                    _allEmployees = context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.Position)
                        .Include(e => e.Manager)
                        .OrderByDescending(e => e.EmployeeID) // Sửa thành EmployeeID
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

            // Sửa logic lọc phòng ban (String)
            if (SelectedDepartment != null && !string.IsNullOrEmpty(SelectedDepartment.DepartmentID))
                query = query.Where(e => e.DepartmentID == SelectedDepartment.DepartmentID);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string key = SearchText.ToLower();
                // Sửa logic tìm kiếm theo FullName
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
        private async Task DeleteEmployee(Employee emp)
        {
            if (emp == null) return;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên {emp.FullName}?\n" + "LƯU Ý: Tài khoản và Hợp đồng liên quan cũng sẽ bị xóa vĩnh viễn!", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new DataContext())
                    {
                        // Sửa thành EmployeeID
                        var dbEmp = context.Employees
                            .Include(e => e.Account)
                            .Include(e => e.WorkContracts)
                            .FirstOrDefault(e => e.EmployeeID == emp.EmployeeID);

                        if (dbEmp != null)
                        {
                            // Lưu ý: Đảm bảo AppSession đã cập nhật để lưu String ID nếu cần
                            // Nếu bảng ChangeHistory bạn đã sửa thành char(5) cho AccountID thì code dưới ok.
                            // Nếu chưa sửa AppSession, tạm thời comment phần ghi log hoặc sửa AppSession sau.

                            // int currentAccountId = AppSession.CurrentUser.AccountId; // Cũ
                            // string currentUserId = AppSession.CurrentUserID; // Mới (bạn cần cập nhật AppSession)

                            // Tạm thời comment ghi log để build được trước đã:
                            /*
                            var history = new ChangeHistory
                            {
                                ActionType = "DELETE",
                                TableName = "Employees",
                                ChangeTime = DateTime.Now,
                                ChangeByUserID = "ADMIN", // Tạm để cứng để test
                                Details = $"Xóa nhân viên: {dbEmp.FullName} (Mã NV: {dbEmp.EmployeeID})"
                            };
                            context.ChangeHistories.Add(history);
                            */

                            if (dbEmp.Account != null) context.Accounts.Remove(dbEmp.Account);
                            if (dbEmp.WorkContracts != null && dbEmp.WorkContracts.Any()) context.WorkContracts.RemoveRange(dbEmp.WorkContracts);

                            var subordinates = context.Employees.Where(e => e.ManagerID == dbEmp.EmployeeID).ToList();
                            foreach (var sub in subordinates) sub.ManagerID = null;

                            context.Employees.Remove(dbEmp);
                            await context.SaveChangesAsync();

                            LoadDataFromDb();
                            MessageBox.Show("Đã xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy nhân viên.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            LoadDataFromDb();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi chi tiết: {ex.Message}", "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}