using System.Windows;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.ViewModels;
using HumanResourcesManagementSystemFinal.Views; // Thêm namespace này để tìm thấy LoginWindow
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

            // Khởi tạo Database
            SeedDatabase();

            // Cấu hình MVVM thủ công
            // Tạo ViewModel 
            var loginVM = new LoginViewModel();
            var loginWindow = new LoginWindow();
            loginWindow.DataContext = loginVM;
            loginWindow.Show();
        }
        private void SeedDatabase()
        {
            try
            {
                using (var context = new DataContext())
                {
                    context.Database.EnsureCreated();

                    // Tạo Role
                    if (!context.Roles.Any())
                    {
                        context.Roles.AddRange(
                            new Role { RoleName = "Admin" },
                            new Role { RoleName = "Employee" }
                        );
                        context.SaveChanges();
                    }

                    // Tạo Chức Vụ
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

                    // Tạo Admin
                    if (!context.Employees.Any(e => e.Email == "admin@hrms.com"))
                    {
                        var managerPos = context.Positions.FirstOrDefault(p => p.Title == "Manager");

                        var empAdmin = new Employee
                        {
                            FirstName = "Quản Trị",
                            LastName = "Viên",
                            Email = "admin@hrms.com",
                            IsActive = true,
                            Gender = "Other",
                            HireDate = DateTime.Now,
                            Position = managerPos
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo dữ liệu: " + ex.Message);
            }
        }
    }
}