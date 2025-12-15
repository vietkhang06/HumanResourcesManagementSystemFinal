using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ManageEmployeeViewModel : ObservableObject
    {
        // Danh sách hiển thị
        public ObservableCollection<Employee> Employees { get; set; } = new();
        public ObservableCollection<Department> Departments { get; set; } = new();

        [ObservableProperty] private string _searchText;
        [ObservableProperty] private Department _selectedDepartment;

        private List<Employee> _allEmployees = new List<Employee>(); // Cache để filter

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
                    // Load Depts
                    var depts = context.Departments.ToList();
                    depts.Insert(0, new Department { Id = 0, DepartmentName = "     --- Tất cả ---" });
                    Departments.Clear();
                    foreach (var d in depts) Departments.Add(d);
                    SelectedDepartment = Departments.FirstOrDefault();

                    // Load Employees + Related Data
                    _allEmployees = context.Employees
                                           .Include(e => e.Department)
                                           .Include(e => e.Position)
                                           .Include(e => e.Manager) // Để hiển thị tên sếp
                                           .OrderByDescending(e => e.Id)
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

            if (SelectedDepartment != null && SelectedDepartment.Id != 0)
                query = query.Where(e => e.DepartmentId == SelectedDepartment.Id);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string key = SearchText.ToLower();
                query = query.Where(e => (e.LastName + " " + e.FirstName).ToLower().Contains(key)
                                      || (e.Email?.ToLower().Contains(key) ?? false));
            }

            Employees.Clear();
            foreach (var e in query) Employees.Add(e);
        }

        [RelayCommand]
        private void AddEmployee()
        {
            // Mở cửa sổ Add
            var addWindow = new AddEmployeeWindow();

            // Nếu người dùng bấm Lưu thành công (DialogResult = true)
            if (addWindow.ShowDialog() == true)
            {
                LoadDataFromDb(); // Tải lại danh sách để thấy nhân viên mới
            }
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp == null) return;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên {emp.FullName}?\n" +
                                         "LƯU Ý: Tài khoản và Hợp đồng liên quan cũng sẽ bị xóa vĩnh viễn!",
                                         "Xác nhận xóa",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new DataContext())
                    {
                        // BƯỚC 1: Tìm nhân viên trong DB và LOAD KÈM dữ liệu liên quan (Include)
                        var dbEmp = context.Employees
                            .Include(e => e.Account)        // Load kèm Account
                            .Include(e => e.WorkContracts)  // Load kèm Hợp đồng
                            .FirstOrDefault(e => e.Id == emp.Id);

                        if (dbEmp != null)
                        {
                            // BƯỚC 2: Xóa Tài khoản liên quan (nếu có)
                            if (dbEmp.Account != null)
                            {
                                context.Accounts.Remove(dbEmp.Account);
                            }

                            // BƯỚC 3: Xóa Hợp đồng liên quan (nếu có)
                            if (dbEmp.WorkContracts != null && dbEmp.WorkContracts.Any())
                            {
                                context.WorkContracts.RemoveRange(dbEmp.WorkContracts);
                            }

                            // BƯỚC 4: Xử lý vụ Self-Referencing (Nếu nhân viên này đang là sếp của ai đó)
                            // Set ManagerId của nhân viên cấp dưới về null để tránh lỗi khóa ngoại
                            var subordinates = context.Employees.Where(e => e.ManagerId == dbEmp.Id).ToList();
                            foreach (var sub in subordinates)
                            {
                                sub.ManagerId = null;
                            }

                            // BƯỚC 5: Xóa chính Nhân viên đó
                            context.Employees.Remove(dbEmp);

                            // BƯỚC 6: Lưu xuống DB (Commit Transaction)
                            context.SaveChanges();

                            // BƯỚC 7: Load lại giao diện
                            LoadDataFromDb();

                            MessageBox.Show("Đã xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy nhân viên trong CSDL. Có thể đã bị xóa trước đó.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            LoadDataFromDb(); // Load lại cho đồng bộ
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Bắt lỗi để App không bị đứng hình
                    MessageBox.Show($"Không thể xóa nhân viên này.\nLỗi chi tiết: {ex.Message}\n{ex.InnerException?.Message}",
                                    "Lỗi Hệ Thống",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }
    }
}