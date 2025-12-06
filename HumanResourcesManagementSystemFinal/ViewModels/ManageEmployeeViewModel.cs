using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ManageEmployeeViewModel : ObservableObject
    {
        private List<Employee> _allEmployees = new List<Employee>();

        public ObservableCollection<Employee> Employees { get; set; } = new();

        public ObservableCollection<Department> Departments { get; set; } = new();

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private Department _selectedDepartment;

        public ManageEmployeeViewModel()
        {
            LoadDataFromDb();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterEmployees();
        }

        partial void OnSelectedDepartmentChanged(Department value)
        {
            FilterEmployees();
        }

        private void FilterEmployees()
        {
            IEnumerable<Employee> query = _allEmployees;

            if (SelectedDepartment != null && SelectedDepartment.Id != 0)
            {
                query = query.Where(e => e.DepartmentId == SelectedDepartment.Id);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.ToLower();
                query = query.Where(e => e.FullName.ToLower().Contains(keyword) ||
                                     (e.Email != null && e.Email.ToLower().Contains(keyword)));
            }

            Employees.Clear();
            foreach (var emp in query)
            {
                Employees.Add(emp);
            }
        }

        private void LoadDataFromDb()
        {
            using (var context = new DataContext())
            {
                context.Database.EnsureCreated();

                var dbDepts = context.Departments.ToList();
                dbDepts.Insert(0, new Department { Id = 0, DepartmentName = "--- Tất cả phòng ban ---" });

                Departments.Clear();
                foreach (var dept in dbDepts) Departments.Add(dept);

                SelectedDepartment = Departments.FirstOrDefault();

                var dbEmps = context.Employees
                                    .Include(e => e.Department)
                                    .Include(e => e.Position)
                                    .Include(e => e.Manager)
                                    .ToList();

                _allEmployees = dbEmps;

                FilterEmployees();
            }
        }

        [RelayCommand]
        private void AddEmployee()
        {
            var validDepts = Departments.Where(d => d.Id != 0).ToList();
            var potentialManagers = _allEmployees.ToList();

            var addWindow = new AddEmployeeWindow(validDepts, potentialManagers);
            if (addWindow.ShowDialog() == true)
            {
                using (var context = new DataContext())
                {
                    var newEmp = addWindow.NewEmployee;

                    int deptId = newEmp.Department.Id;
                    newEmp.Department = null;
                    newEmp.DepartmentId = deptId;

                    Position existPos = null;
                    if (newEmp.Position != null)
                    {
                        existPos = context.Positions.FirstOrDefault(p => p.Title == newEmp.Position.Title);
                    }

                    newEmp.Manager = null;

                    newEmp.WorkContracts = new List<WorkContract>
                    {
                        new WorkContract { Salary = addWindow.BaseSalary, StartDate = addWindow.StartDate, EndDate = addWindow.StartDate.AddYears(1), ContractType = "Full-Time" }
                    };

                    if (existPos != null)
                    {
                        newEmp.Position = null;
                        newEmp.PositionId = existPos.Id;
                    }
                    else
                    {
                        if (newEmp.Position != null && string.IsNullOrEmpty(newEmp.Position.JobDescription))
                        {
                            newEmp.Position.JobDescription = "Mô tả công việc mặc định";
                        }
                    }

                    context.Employees.Add(newEmp);
                    context.SaveChanges();

                    if (!string.IsNullOrEmpty(addWindow.SelectedImagePath)) SaveImage(newEmp.Id, addWindow.SelectedImagePath);

                    newEmp.Department = Departments.FirstOrDefault(d => d.Id == deptId);
                    if (newEmp.ManagerId != null) newEmp.Manager = _allEmployees.FirstOrDefault(e => e.Id == newEmp.ManagerId);
                    if (newEmp.PositionId != null) newEmp.Position = context.Positions.Find(newEmp.PositionId);

                    _allEmployees.Add(newEmp);
                    FilterEmployees();
                }
            }
        }

        [RelayCommand]
        private void EditEmployee(Employee emp)
        {
            if (emp == null) return;

            var validDepts = Departments.Where(d => d.Id != 0).ToList();
            var potentialManagers = _allEmployees.ToList();

            var editWindow = new AddEmployeeWindow(validDepts, potentialManagers, emp);
            if (editWindow.ShowDialog() == true)
            {
                using (var context = new DataContext())
                {
                    var empInDb = context.Employees.Include(e => e.Position).FirstOrDefault(e => e.Id == emp.Id);
                    if (empInDb != null)
                    {
                        var updated = editWindow.NewEmployee;

                        empInDb.FirstName = updated.FirstName;
                        empInDb.LastName = updated.LastName;
                        empInDb.Email = updated.Email;
                        empInDb.PhoneNumber = updated.PhoneNumber;
                        empInDb.Address = updated.Address;
                        empInDb.DepartmentId = updated.DepartmentId;
                        empInDb.ManagerId = updated.ManagerId;

                        if (updated.Position != null)
                        {
                            var existPos = context.Positions.FirstOrDefault(p => p.Title == updated.Position.Title);
                            if (existPos != null)
                            {
                                empInDb.Position = null;
                                empInDb.PositionId = existPos.Id;
                            }
                            else
                            {
                                var newPos = new Position { Title = updated.Position.Title };
                                if (string.IsNullOrEmpty(updated.Position.JobDescription))
                                {
                                    newPos.JobDescription = "Mô tả công việc mặc định";
                                }
                                else
                                {
                                    newPos.JobDescription = updated.Position.JobDescription;
                                }
                                empInDb.Position = newPos;
                            }
                        }

                        context.SaveChanges();

                        if (!string.IsNullOrEmpty(editWindow.SelectedImagePath)) SaveImage(emp.Id, editWindow.SelectedImagePath);

                        _allEmployees = context.Employees
                                               .Include(e => e.Department)
                                               .Include(e => e.Position)
                                               .Include(e => e.Manager)
                                               .ToList();

                        FilterEmployees();

                        MessageBox.Show("Cập nhật thành công!", "Thông báo");
                    }
                }
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
                    context.Employees.Remove(emp);
                    context.SaveChanges();

                    _allEmployees.Remove(emp);
                    FilterEmployees();

                    DeleteImage(emp.Id);
                }
            }
        }

        private void SaveImage(int empId, string sourcePath)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string ext = Path.GetExtension(sourcePath);
                File.Copy(sourcePath, Path.Combine(folder, $"{empId}{ext}"), true);
            }
            catch { }
        }

        private void DeleteImage(int empId)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                if (File.Exists(Path.Combine(folder, $"{empId}.png"))) File.Delete(Path.Combine(folder, $"{empId}.png"));
                if (File.Exists(Path.Combine(folder, $"{empId}.jpg"))) File.Delete(Path.Combine(folder, $"{empId}.jpg"));
            }
            catch { }
        }
    }
}