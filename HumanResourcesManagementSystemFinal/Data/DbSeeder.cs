using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Linq;

namespace HumanResourcesManagementSystemFinal.Data
{
    public static class DbSeeder
    {
        public static void Seed(DataContext context)
        {
            context.Database.EnsureCreated();

            // 1. Seed Roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleID = "R001", RoleName = "Admin" },
                    new Role { RoleID = "R002", RoleName = "Manager" },
                    new Role { RoleID = "R003", RoleName = "Employee" }
                );
                context.SaveChanges();
            }

            // 2. Seed Departments
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { DepartmentID = "PB001", DepartmentName = "Ban Giám Đốc", Location = "Tầng 5" },
                    new Department { DepartmentID = "PB002", DepartmentName = "Nhân Sự", Location = "Tầng 2" },
                    new Department { DepartmentID = "PB003", DepartmentName = "Kỹ Thuật", Location = "Tầng 3" }
                );
                context.SaveChanges();
            }

            // 3. Seed Positions
            if (!context.Positions.Any())
            {
                context.Positions.AddRange(
                    new Position { PositionID = "CV001", PositionName = "CEO" },
                    new Position { PositionID = "CV002", PositionName = "HR Manager" },
                    new Position { PositionID = "CV003", PositionName = "Developer" }
                );
                context.SaveChanges();
            }

            // 4. Seed Employee & Account
            if (!context.Employees.Any())
            {
                var adminEmp = new Employee
                {
                    EmployeeID = "NV001",
                    FullName = "Super Admin",
                    Status = "Active", // Sửa IsActive -> Status
                    DepartmentID = "PB001",
                    PositionID = "CV001",
                    Email = "admin@company.com",
                };

                context.Employees.Add(adminEmp);

                var adminAcc = new Account
                {
                    UserID = "TK001",
                    UserName = "admin",
                    Password = "123", // Sửa PasswordHash -> Password
                    IsActive = "Active", // Sửa bool -> string
                    EmployeeID = "NV001",
                    RoleID = "R001"
                };

                context.Accounts.Add(adminAcc);
                context.SaveChanges();
            }
        }
    }
}