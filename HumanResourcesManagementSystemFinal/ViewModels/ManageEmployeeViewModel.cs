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
                    depts.Insert(0, new Department { Id = 0, DepartmentName = "      --- Tất cả ---" });
                    Departments.Clear();
                    foreach (var d in depts) Departments.Add(d);
                    SelectedDepartment = Departments.FirstOrDefault();
                    _allEmployees = context.Employees.Include(e => e.Department).Include(e => e.Position).Include(e => e.Manager).OrderByDescending(e => e.Id).ToList();
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
                query = query.Where(e => (e.LastName + " " + e.FirstName).ToLower().Contains(key) || (e.Email?.ToLower().Contains(key) ?? false));
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
                        var dbEmp = context.Employees.Include(e => e.Account).Include(e => e.WorkContracts).FirstOrDefault(e => e.Id == emp.Id);
                        if (dbEmp != null)
                        {
                            int currentAccountId = AppSession.CurrentUser.Id;

                            var history = new ChangeHistory
                            {
                                ActionType = "DELETE",
                                TableName = "Employees",
                                ChangeTime = DateTime.Now,
                                AccountId = currentAccountId,
                                Details = $"Xóa nhân viên: {dbEmp.FirstName} {dbEmp.LastName} (Mã NV: {dbEmp.Id})"
                            };
                            context.ChangeHistories.Add(history);

                            if (dbEmp.Account != null)
                            {
                                context.Accounts.Remove(dbEmp.Account);
                            }
                            if (dbEmp.WorkContracts != null && dbEmp.WorkContracts.Any())
                            {
                                context.WorkContracts.RemoveRange(dbEmp.WorkContracts);
                            }
                            var subordinates = context.Employees.Where(e => e.ManagerId == dbEmp.Id).ToList();
                            foreach (var sub in subordinates)
                            {
                                sub.ManagerId = null;
                            }
                            context.Employees.Remove(dbEmp);
                            await context.SaveChangesAsync();
                            LoadDataFromDb();
                            MessageBox.Show("Đã xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy nhân viên trong CSDL. Có thể đã bị xóa trước đó.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            LoadDataFromDb();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể xóa nhân viên này.\nLỗi chi tiết: {ex.Message}\n{ex.InnerException?.Message}",
                                    "Lỗi Hệ Thống",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }
    }
}