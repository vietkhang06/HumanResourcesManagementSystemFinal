using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ManageEmployeeViewModel : ObservableObject
    {
        private List<Employee> _allEmployees = new();
        public ObservableCollection<Employee> Employees { get; } = new();
        public ObservableCollection<Department> Departments { get; } = new();

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private Department _selectedDepartment;
        [ObservableProperty] private Employee _selectedEmployee;

        public ManageEmployeeViewModel()
        {
            LoadDataFromDb();
        }

        public void LoadDataFromDb()
        {
            try
            {
                using var context = new DataContext();

                // 1. Tải danh sách phòng ban
                var deptList = context.Departments.ToList();
                deptList.Insert(0, new Department
                {
                    DepartmentID = "",
                    DepartmentName = "Tất cả"
                });

                Departments.Clear();
                foreach (var dept in deptList)
                    Departments.Add(dept);

                if (SelectedDepartment == null)
                    SelectedDepartment = Departments.FirstOrDefault();

                // 2. Tải danh sách nhân viên
                // --- SỬA LỖI QUAN TRỌNG Ở ĐÂY ---
                _allEmployees = context.Employees
                    .AsNoTracking()
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.Manager)
                    .Include(e => e.WorkContracts)
                    .Include(e => e.Account) // <--- BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ LẤY ẢNH
                    .OrderByDescending(e => e.EmployeeID)
                    .ToList();
                // -------------------------------

                // 3. LẤY DỮ LIỆU CHẤM CÔNG HÔM NAY
                var today = DateTime.Today;

                var todayTimeSheets = context.TimeSheets
                    .Where(t => t.WorkDate == today)
                    .ToList();

                // 4. CẬP NHẬT TRẠNG THÁI HIỂN THỊ
                foreach (var emp in _allEmployees)
                {
                    if (emp.Status == "Resigned" || emp.Status == "Đã nghỉ việc")
                        continue;

                    var timesheet = todayTimeSheets.FirstOrDefault(t => t.EmployeeID == emp.EmployeeID);

                    if (timesheet == null || timesheet.TimeIn == null)
                    {
                        emp.Status = "Chưa vào làm";
                    }
                    else if (timesheet.TimeOut != null)
                    {
                        emp.Status = "Đã tan làm";
                    }
                    else
                    {
                        emp.Status = "Đang làm việc";
                    }
                }

                FilterEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private void AddEmployee() => ShowAddEmployeeWindow(null);

        [RelayCommand]
        private void EditEmployee(Employee emp)
        {
            if (emp != null) ShowAddEmployeeWindow(emp);
        }

        private void ShowAddEmployeeWindow(Employee existingEmp)
        {
            var addVM = existingEmp != null
                ? new AddEmployeeViewModel(existingEmp)
                : new AddEmployeeViewModel();

            var window = new AddEmployeeWindow { DataContext = addVM };

            if (window.ShowDialog() == true)
            {
                LoadDataFromDb();
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
                    if (dbEmp.Account != null) context.Accounts.Remove(dbEmp.Account);
                    if (dbEmp.WorkContracts != null) context.WorkContracts.RemoveRange(dbEmp.WorkContracts);

                    var subs = context.Employees.Where(e => e.ManagerID == dbEmp.EmployeeID);
                    foreach (var sub in subs) sub.ManagerID = null;

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

        [RelayCommand]
        private void ViewDetail(Employee emp)
        {
            if (emp == null) return;

            var detailVM = new EmployeeDetailViewModel(emp);
            var detailWindow = new EmployeeDetailWindow
            {
                DataContext = detailVM
            };
            detailWindow.ShowDialog();
        }
        [RelayCommand]
        private void ExportToExcel()
        {
            // 1. Kiểm tra danh sách có dữ liệu không
            if (Employees == null || Employees.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Cấu hình hộp thoại lưu file
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel CSV (*.csv)|*.csv",
                FileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();

                    // 3. Tạo dòng tiêu đề (Header)
                    sb.AppendLine("Ma NV,Ho Ten,Email,Phong Ban,Chuc Vu,Trang Thai");

                    // 4. Duyệt danh sách nhân viên để lấy dữ liệu
                    foreach (var emp in Employees)
                    {
                        string department = emp.Department?.DepartmentName ?? "N/A";
                        string position = emp.Position?.PositionName ?? "N/A";

                        // Gom dữ liệu thành một dòng, phân cách bằng dấu phẩy
                        sb.AppendLine($"{emp.EmployeeID},{emp.FullName},{emp.Email},{department},{position},{emp.Status}");
                    }

                    // 5. Ghi file với định dạng UTF-8 để không lỗi font tiếng Việt
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Xuất danh sách nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}