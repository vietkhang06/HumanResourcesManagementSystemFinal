using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data; 
namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ManageEmployeeViewModel : ObservableObject
    {
        public ObservableCollection<Employee> Employees { get; set; } = new();

        public ObservableCollection<Department> Departments { get; set; } = new();

        [ObservableProperty]
        private string _searchText;

        public ManageEmployeeViewModel()
        {
            LoadDataFromDb();
        }

        private void LoadDataFromDb()
        {
            using (var context = new DataContext())
            {
                context.Database.EnsureCreated();
                var dbDepts = context.Departments.ToList();
                Departments.Clear();
                foreach (var dept in dbDepts) Departments.Add(dept);

                var dbEmps = context.Employees
                                    .Include(e => e.Department)
                                    .Include(e => e.Position)
                                    .Include(e => e.Manager) 
                                    .ToList();

                Employees.Clear();
                foreach (var emp in dbEmps) Employees.Add(emp);
            }
        }

        [RelayCommand]
        private void AddEmployee()
        {
            var potentialManagers = Employees.ToList();

            var addWindow = new AddEmployeeWindow(Departments, potentialManagers);
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                using (var context = new DataContext())
                {
                    var newEmp = addWindow.NewEmployee;
                    int deptId = newEmp.Department.Id;
                    newEmp.Department = null;
                    newEmp.DepartmentId = deptId;
                    if (newEmp.Position != null)
                    {
                        var existPos = context.Positions.FirstOrDefault(p => p.Title == newEmp.Position.Title);
                        if (existPos != null)
                        {
                            newEmp.Position = null; 
                            newEmp.PositionId = existPos.Id; 
                        }
                    }
                    newEmp.Manager = null;
                    var contract = new WorkContract
                    {
                        Salary = addWindow.BaseSalary,
                        StartDate = addWindow.StartDate,
                        EndDate = addWindow.StartDate.AddYears(1), 
                        ContractType = "Full-Time"
                    };
               
                    newEmp.WorkContracts = new List<WorkContract> { contract };
                    context.Employees.Add(newEmp);
                    context.SaveChanges();
                    if (!string.IsNullOrEmpty(addWindow.SelectedImagePath))
                    {
                        SaveImage(newEmp.Id, addWindow.SelectedImagePath);
                    }
                    newEmp.Department = Departments.FirstOrDefault(d => d.Id == deptId);
                    if (newEmp.ManagerId != null)
                    {
                        newEmp.Manager = Employees.FirstOrDefault(e => e.Id == newEmp.ManagerId);
                    }
                    if (newEmp.Position == null && newEmp.PositionId != null)
                    {
                        newEmp.Position = context.Positions.Find(newEmp.PositionId);
                    }

                    Employees.Add(newEmp);
                }
            }
        }

       [RelayCommand]
        private void EditEmployee(Employee emp)
        {
            if (emp == null) return;

            var potentialManagers = Employees.ToList();

            var editWindow = new AddEmployeeWindow(Departments, potentialManagers, emp);
    
            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                using (var context = new DataContext())
                {
                    var empInDb = context.Employees.Include(e => e.Position).FirstOrDefault(e => e.Id == emp.Id);

                    if (empInDb != null)
                    {
                        var updatedInfo = editWindow.NewEmployee;

                        empInDb.FirstName = updatedInfo.FirstName;
                        empInDb.LastName = updatedInfo.LastName;
                        empInDb.Email = updatedInfo.Email;
                        empInDb.PhoneNumber = updatedInfo.PhoneNumber;
                        empInDb.Address = updatedInfo.Address;
                        empInDb.DepartmentId = updatedInfo.DepartmentId;
                        empInDb.ManagerId = updatedInfo.ManagerId;
                        if (updatedInfo.Position != null)
                        {
                            var existPos = context.Positions.FirstOrDefault(p => p.Title == updatedInfo.Position.Title);
                            if (existPos != null)
                            {
                                empInDb.Position = null;
                                empInDb.PositionId = existPos.Id; 
                            }
                            else
                            {
                                empInDb.Position = new Position { Title = updatedInfo.Position.Title };
                            }
                        }

                        context.SaveChanges();
                        emp.FirstName = empInDb.FirstName;
                        emp.LastName = empInDb.LastName;
                        emp.Email = empInDb.Email;
                        emp.PhoneNumber = empInDb.PhoneNumber;
                        emp.Address = empInDb.Address;
                        emp.Department = Departments.FirstOrDefault(d => d.Id == empInDb.DepartmentId);
                        emp.Manager = Employees.FirstOrDefault(e => e.Id == empInDb.ManagerId);
                
                        if (empInDb.PositionId != null)
                        {
                            emp.Position = context.Positions.Find(empInDb.PositionId);
                        }
                        if (!string.IsNullOrEmpty(editWindow.SelectedImagePath))
                        {
                            SaveImage(emp.Id, editWindow.SelectedImagePath);
                        }
                    }
                }
            }
        }

        [RelayCommand]
        private void DeleteEmployee(Employee emp)
        {
            if (emp == null) return;

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa {emp.FullName}?",
                                          "Xác nhận xóa",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                using (var context = new DataContext())
                {
                    context.Employees.Remove(emp);
                    context.SaveChanges();
                    Employees.Remove(emp);
                    DeleteImage(emp.Id);
                }
            }
        }
        partial void OnSearchTextChanged(string value)
        {
            var view = CollectionViewSource.GetDefaultView(Employees);

            if (string.IsNullOrWhiteSpace(value))
            {
                view.Filter = null; 
            }
            else
            {
                view.Filter = (obj) =>
                {
                    if (obj is Employee e)
                    {
                        string keyword = value.ToLower();
                        return e.FullName.ToLower().Contains(keyword) ||
                               (e.Email != null && e.Email.ToLower().Contains(keyword));
                    }
                    return false;
                };
            }
        }

        private void SaveImage(int empId, string sourcePath)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string ext = Path.GetExtension(sourcePath);
                string destPath = Path.Combine(folder, $"{empId}{ext}");

                File.Copy(sourcePath, destPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu ảnh: " + ex.Message);
            }
        }

        private void DeleteImage(int empId)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                string png = Path.Combine(folder, $"{empId}.png");
                string jpg = Path.Combine(folder, $"{empId}.jpg");

                if (File.Exists(png)) File.Delete(png);
                if (File.Exists(jpg)) File.Delete(jpg);
            }
            catch { /* Ignore error */ }
        }
    }
}