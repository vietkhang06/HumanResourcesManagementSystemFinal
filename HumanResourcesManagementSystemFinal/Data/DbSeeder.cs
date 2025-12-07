using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Linq;

namespace HumanResourcesManagementSystemFinal.Data
{
    public static class DbSeeder
    {
        public static void Seed(DataContext context)
        {
            var empRole = context.Roles.FirstOrDefault(r => r.RoleName == "Employee");
            if (empRole == null)
            {
                empRole = new Role { RoleId = 2, RoleName = "Employee"};
                context.Roles.Add(empRole);
                context.SaveChanges();
            }
            var testEmployee = context.Employees.FirstOrDefault(e => e.Email == "user@test.com");
            if (testEmployee == null)
            {
                testEmployee = new Employee
                {
                    FirstName = "User",
                    LastName = "Test",
                    Email = "user@test.com",
                    PhoneNumber = "0999888777",
                    IsActive = true,
                    HireDate = DateTime.Now,
                    DepartmentId = 1,
                    PositionId = 2
                };
                context.Employees.Add(testEmployee);
                context.SaveChanges();
            }

            if (!context.Accounts.Any(a => a.Username == "user"))
            {
                var userAccount = new Account
                {
                    Username = "user",
                    PasswordHash = "123",
                    EmployeeId = testEmployee.Id,
                    RoleId = empRole.RoleId,    
                    IsActive = true
                };
                context.Accounts.Add(userAccount);
                context.SaveChanges();
            }
        }
    }
}