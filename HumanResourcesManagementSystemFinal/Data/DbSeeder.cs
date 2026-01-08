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

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleID = "R001", RoleName = "Admin" },
                    new Role { RoleID = "R002", RoleName = "Employee" }
                );
                context.SaveChanges();
            }

            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { DepartmentID = "PB001", DepartmentName = "Ban Giám Đốc", Location = "Tầng 5" },
                    new Department { DepartmentID = "PB002", DepartmentName = "Nhân Sự", Location = "Tầng 2" },
                    new Department { DepartmentID = "PB003", DepartmentName = "Kỹ Thuật", Location = "Tầng 3" }
                );
                context.SaveChanges();
            }


            if (!context.Positions.Any())
            {
                context.Positions.AddRange(
                    new Position { PositionID = "CV001", PositionName = "CEO", DepartmentID = "PB001" },
                    new Position { PositionID = "CV002", PositionName = "HR Manager", DepartmentID = "PB002" },
                    new Position { PositionID = "CV003", PositionName = "Developer", DepartmentID = "PB003" }
                );
                context.SaveChanges();
            }

            if (!context.Employees.Any())
            {
                var adminEmp = new Employee
                {
                    EmployeeID = "NV001",
                    FullName = "Super Admin",
                    Status = "Active",
                    DepartmentID = "PB001",
                    PositionID = "CV001",
                    Email = "admin@company.com",
                };

                context.Employees.Add(adminEmp);

                var adminAcc = new Account
                {
                    UserID = "TK001",
                    UserName = "admin",
                    Password = "123",
                    IsActive = "Active",
                    EmployeeID = "NV001",
                    RoleID = "R001"
                };

                context.Accounts.Add(adminAcc);
                context.SaveChanges();
            }
        }
    }
}