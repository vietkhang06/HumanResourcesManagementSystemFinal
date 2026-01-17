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

                // 1. Tải phòng ban
                var deptList = context.Departments.ToList();
                deptList.Insert(0, new Department
                {
                    DepartmentID = "",
                    DepartmentName = "Tất cả"
                });

                Departments.Clear();
                foreach (var dept in deptList)
                    Departments.Add(dept);

                // Giữ lại lựa chọn cũ nếu có, nếu không thì chọn cái đầu tiên
                if (SelectedDepartment == null)
                    SelectedDepartment = Departments.FirstOrDefault();

                // 2. Tải nhân viên
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
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Tự động lọc khi gõ chữ hoặc chọn phòng ban
        partial void OnSearchTextChanged(string value) => FilterEmployees();
        partial void OnSelectedDepartmentChanged(Department value) => FilterEmployees();

        private void FilterEmployees()
        {
            IEnumerable<Employee> query = _allEmployees;

            // Lọc theo phòng ban
            if (SelectedDepartment != null && !string.IsNullOrEmpty(SelectedDepartment.DepartmentID))
            {
                query = query.Where(e => e.DepartmentID == SelectedDepartment.DepartmentID);
            }

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.ToLower();
                query = query.Where(e =>
                    (e.FullName ?? "").ToLower().Contains(keyword) ||
                    (e.Email ?? "").ToLower().Contains(keyword) ||
                    (e.EmployeeID ?? "").ToLower().Contains(keyword));
            }

            // Cập nhật lên giao diện
            Employees.Clear();
            foreach (var emp in query)
                Employees.Add(emp);
        }

        [RelayCommand]
        private void AddEmployee() => ShowAddEmployeeWindow(null);

        [RelayCommand]
        private void EditEmployee(Employee emp)
        {
            if (emp != null) ShowAddEmployeeWindow(emp);
        }

        // Hàm chung để mở cửa sổ Thêm/Sửa
        private void ShowAddEmployeeWindow(Employee existingEmp)
        {
            var addVM = existingEmp != null
                ? new AddEmployeeViewModel(existingEmp)
                : new AddEmployeeViewModel();

            var window = new AddEmployeeWindow { DataContext = addVM };

            if (window.ShowDialog() == true)
            {
                LoadDataFromDb(); // Tải lại danh sách sau khi thêm/sửa
            }
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

                if (dbEmp != null)
                {
                    // Xóa các bảng phụ trước
                    if (dbEmp.Account != null) context.Accounts.Remove(dbEmp.Account);
                    if (dbEmp.WorkContracts != null) context.WorkContracts.RemoveRange(dbEmp.WorkContracts);

                    // Cập nhật nhân viên cấp dưới (bỏ ManagerID)
                    var subs = context.Employees.Where(e => e.ManagerID == dbEmp.EmployeeID);
                    foreach (var sub in subs) sub.ManagerID = null;

                    // Ghi lịch sử
                    context.ChangeHistories.Add(new ChangeHistory
                    {
                        LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                        TableName = "Employees",
                        ActionType = "DELETE",
                        RecordID = dbEmp.EmployeeID,
                        ChangeByUserID = UserSession.CurrentEmployeeId ?? "ADMIN",
                        ChangeTime = DateTime.Now,
                        Details = $"Xóa nhân viên: {dbEmp.FullName}"
                    });

                    // Xóa nhân viên
                    context.Employees.Remove(dbEmp);
                    await context.SaveChangesAsync();

                    LoadDataFromDb();
                    MessageBox.Show("Đã xóa thành công!", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // [QUAN TRỌNG] Đổi tên thành ViewDetail để khớp với lỗi CS1061
        [RelayCommand]
        private void ViewDetail(Employee emp)
        {
            if (emp == null)
            {
                MessageBox.Show("Nhưng chưa lấy được thông tin nhân viên (emp bị null)");
                return;
            }
            // 2. Tạo ViewModel và Window
            var detailVM = new EmployeeDetailViewModel(emp);
            var detailWindow = new EmployeeDetailWindow
            {
                DataContext = detailVM
            };

            // 3. Hiện cửa sổ
            detailWindow.ShowDialog();
        }
    }
}