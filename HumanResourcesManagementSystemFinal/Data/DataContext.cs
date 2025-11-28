using Microsoft.EntityFrameworkCore;
using HumanResourcesManagementSystemFinal.Models; // Nhớ using Models

namespace HumanResourcesManagementSystemFinal.Data
{
    public class DataContext : DbContext
    {
        // 1. Khai báo tên file Database
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Database sẽ được tạo ngay tại thư mục bin/Debug/...
            optionsBuilder.UseSqlite("Data Source=HRMS_Pro.db");
        }

        // 2. Khai báo các bảng (DbSet) khớp với Models của bạn
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Role> Roles { get; set; }
        // Thêm các bảng khác nếu có (LeaveRequest, TimeSheet...)

        // 3. Tạo dữ liệu mẫu (Seed Data) - QUAN TRỌNG ĐỂ TEST ĐĂNG NHẬP
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tạo sẵn một Role Admin
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin"},
                new Role { RoleId = 2, RoleName = "Employee"}
            );

            // Tạo sẵn tài khoản Admin (User: admin / Pass: 123)
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    AccountId = 1,
                    Username = "admin",
                    PasswordHash = "123", // Lưu ý: Dự án thật phải mã hóa password
                    IsActive = true,
                    RoleId = 1,
                    EmployeeId = null // Admin hệ thống có thể không cần liên kết nhân viên
                }
            );
        }
    }
}