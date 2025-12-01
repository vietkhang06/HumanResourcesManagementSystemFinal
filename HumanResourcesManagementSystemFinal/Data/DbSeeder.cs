using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Linq;

namespace HumanResourcesManagementSystemFinal.Data
{
    public static class DbSeeder
    {
        // Hàm này sẽ được gọi khi App khởi động
        public static void Seed(DataContext context)
        {
            // 1. Đảm bảo Role "Employee" (Nhân viên) đã tồn tại
            var empRole = context.Roles.FirstOrDefault(r => r.RoleName == "Employee");
            if (empRole == null)
            {
                // Nếu chưa có thì tạo mới (RoleId = 2)
                empRole = new Role { RoleId = 2, RoleName = "Employee"};
                context.Roles.Add(empRole);
                context.SaveChanges();
            }

            // 2. Tạo một Nhân viên mẫu tên là "Test User"
            var testEmployee = context.Employees.FirstOrDefault(e => e.Email == "user@test.com");
            if (testEmployee == null)
            {
                testEmployee = new Employee
                {
                    // Lưu ý: ID tự sinh
                    FirstName = "User",
                    LastName = "Test",
                    Email = "user@test.com",
                    PhoneNumber = "0999888777",
                    IsActive = true,
                    HireDate = DateTime.Now,
                    // Giả sử DepartmentId 1 (IT) và PositionId 2 (Dev) đã có
                    // Nếu chưa có bạn có thể cần tạo thêm hoặc để null nếu db cho phép
                    DepartmentId = 1,
                    PositionId = 2
                };
                context.Employees.Add(testEmployee);
                context.SaveChanges();
            }

            // 3. Cấp tài khoản đăng nhập cho nhân viên này (User: user / Pass: 123)
            if (!context.Accounts.Any(a => a.Username == "user"))
            {
                var userAccount = new Account
                {
                    Username = "user",
                    PasswordHash = "123",
                    EmployeeId = testEmployee.Id, // Liên kết với nhân viên vừa tạo
                    RoleId = empRole.RoleId,      // Quan trọng: Gán quyền Employee (RoleId = 2)
                    IsActive = true
                };
                context.Accounts.Add(userAccount);
                context.SaveChanges();
            }
        }
    }
}