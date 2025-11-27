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
        // Sự kiện này chạy đầu tiên khi bấm Start
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Khởi tạo Database
            SeedDatabase();

            // 2. Cấu hình MVVM thủ công
            // Tạo ViewModel trước
            var loginVM = new LoginViewModel();

            // SỬA LỖI CS1729: Dùng constructor mặc định và gán DataContext sau
            // Cách này an toàn hơn nếu bạn chưa kịp sửa constructor bên LoginWindow.xaml.cs
            var loginWindow = new LoginWindow();
            loginWindow.DataContext = loginVM;

            // 3. QUAN TRỌNG NHẤT: Phải lệnh cho nó hiện lên
            loginWindow.Show();
        }

        // Hàm tạo dữ liệu mẫu (Giữ nguyên logic cũ của bạn)
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

                            // SỬA LỖI CS0117: Thay Status (string) bằng IsActive (bool)
                            IsActive = true,
                            // Status = "Active", // <-- Dòng này gây lỗi vì Model không có thuộc tính Status

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
                                RoleId = adminRole.Id,
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