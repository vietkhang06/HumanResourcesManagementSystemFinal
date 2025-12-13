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
            if (MessageBox.Show($"Xóa nhân viên {emp.FullName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (var context = new DataContext())
                {
                    // Cần load kèm Account và Contracts để xóa (Cascade delete thủ công nếu DB chưa set)
                    var target = context.Employees
                        .Include(e => e.Account)
                        .Include(e => e.WorkContracts)
                        .FirstOrDefault(e => e.Id == emp.Id);

                    if (target != null)
                    {
                        if (target.Account != null) context.Accounts.Remove(target.Account);
                        context.WorkContracts.RemoveRange(target.WorkContracts);
                        context.Employees.Remove(target);
                        context.SaveChanges();

                        LoadDataFromDb();
                    }
                }
            }
        }
    }
}