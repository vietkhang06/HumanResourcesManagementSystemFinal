using System.Windows;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.ViewModels;
using HumanResourcesManagementSystemFinal.Views; 
using Microsoft.EntityFrameworkCore;
using System.Linq;
using HumanResourcesManagementSystemFinal.Models;
using System;

namespace HumanResourcesManagementSystemFinal
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SeedDatabase();
            var loginVM = new LoginViewModel();
            var loginWindow = new LoginWindow();
            loginWindow.DataContext = loginVM;
        }
        private void SeedDatabase()
        {
            try
            {
                using (var context = new DataContext())
                {
                    context.Database.EnsureCreated();

                    if (!context.Roles.Any())
                    {
                        context.Roles.AddRange(
                            new Role { RoleName = "Admin" },
                            new Role { RoleName = "Employee" }
                        );
                        context.SaveChanges();
                    }
                    if (!context.Positions.Any())
                    {
                        context.Positions.AddRange(
                            new Position { Title = "Intern", JobDescription = "Thực tập sinh" },
                            new Position { Title = "Junior", JobDescription = "Nhân viên sơ cấp" },
                            new Position { Title = "Senior", JobDescription = "Nhân viên cao cấp" },
                            new Position { Title = "Manager", JobDescription = "Quản lý" },
                            new Position { Title = "Director", JobDescription = "Giám đốc" }
                        );
                        context.SaveChanges();
                    }
                    if (!context.Departments.Any())
                    {
                        context.Departments.AddRange(
                            new Department { DepartmentName = "Phòng IT", Location = "Tầng 3" },
                            new Department { DepartmentName = "Phòng Nhân Sự", Location = "Tầng 2" },
                            new Department { DepartmentName = "Phòng Kinh Doanh", Location = "Tầng 1" }
                        );
                        context.SaveChanges();
                    }
                    if (!context.Employees.Any(e => e.Email == "admin@hrms.com"))
                    {
                        var managerPos = context.Positions.FirstOrDefault(p => p.Title == "Manager");
                        var itDept = context.Departments.FirstOrDefault(d => d.DepartmentName == "Phòng IT");

                        var empAdmin = new Employee
                        {
                            FirstName = "Quản Trị",
                            LastName = "Viên",
                            Email = "admin@hrms.com",
                            IsActive = true,
                            Gender = "Other",
                            HireDate = DateTime.Now,
                            PositionId = managerPos?.Id,   
                            DepartmentId = itDept?.Id    
                        };
                        context.Employees.Add(empAdmin);
                        context.SaveChanges();

                        var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                        if (adminRole != null)
                        {
                            var accAdmin = new Account
                            {
                                EmployeeId = empAdmin.Id,
                                Username = "admin",
                                PasswordHash = "123",
                                RoleId = adminRole.RoleId,
                                IsActive = true
                            };
                            context.Accounts.Add(accAdmin);
                            context.SaveChanges();
                        }
                    }

                    if (!context.Employees.Any(e => e.Email == "user@test.com"))
                    {
                        var juniorPos = context.Positions.FirstOrDefault(p => p.Title == "Junior");
                        var hrDept = context.Departments.FirstOrDefault(d => d.DepartmentName == "Phòng Nhân Sự");

                        var testEmp = new Employee
                        {
                            FirstName = "User",
                            LastName = "Test",
                            Email = "user@test.com",
                            PhoneNumber = "0988888888",
                            IsActive = true,
                            HireDate = DateTime.Now,
                            PositionId = juniorPos?.Id,
                            DepartmentId = hrDept?.Id
                        };
                        context.Employees.Add(testEmp);
                        context.SaveChanges();

                        var empRole = context.Roles.FirstOrDefault(r => r.RoleName == "Employee");
                        if (empRole != null)
                        {
                            var userAccount = new Account
                            {
                                Username = "user",
                                PasswordHash = "123",
                                IsActive = true,
                                EmployeeId = testEmp.Id,
                                RoleId = empRole.RoleId
                            };
                            context.Accounts.Add(userAccount);
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo dữ liệu: " + ex.Message);
            }
        }
    }
}