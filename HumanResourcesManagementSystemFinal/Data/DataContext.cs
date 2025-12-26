using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace HumanResourcesManagementSystemFinal.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<WorkContract> WorkContracts { get; set; }
        public DbSet<TimeSheet> TimeSheets { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<ChangeHistory> ChangeHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Thiết lập file database SQLite
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HRMS.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. Cấu hình Quan hệ Phòng ban - Nhân viên ---
            // Một Nhân viên thuộc một Phòng ban
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentID)
                .OnDelete(DeleteBehavior.SetNull); // Nếu xóa phòng, nhân viên không bị xóa (DepartmentID = null)

            // --- 2. Cấu hình Quan hệ Chức vụ - Nhân viên ---
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionID)
                .OnDelete(DeleteBehavior.SetNull);

            // --- 3. Cấu hình Quan hệ Tài khoản - Nhân viên (1-1) ---
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Employee)
                .WithOne(e => e.Account)
                .HasForeignKey<Account>(a => a.EmployeeID)
                .OnDelete(DeleteBehavior.Cascade); // Xóa nhân viên -> Xóa luôn tài khoản

            // --- 4. Cấu hình Quan hệ Vai trò - Tài khoản ---
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(a => a.RoleID);

            // --- 5. Cấu hình Hợp đồng - Nhân viên ---
            modelBuilder.Entity<WorkContract>()
                .HasOne(c => c.Employee)
                .WithMany(e => e.WorkContracts)
                .HasForeignKey(c => c.EmployeeID)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 6. Cấu hình Chấm công - Nhân viên ---
            modelBuilder.Entity<TimeSheet>()
                .HasOne(t => t.Employee)
                .WithMany(e => e.TimeSheets)
                .HasForeignKey(t => t.EmployeeID)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 7. Cấu hình Nghỉ phép - Nhân viên ---
            // Người gửi yêu cầu
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Requester)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeID)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 8. Cấu hình Tự tham chiếu (Quản lý - Nhân viên) ---
            // Một nhân viên có thể có 1 người quản lý (cũng là nhân viên)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany() // Không cần danh sách ngược lại ở Manager
                .HasForeignKey(e => e.ManagerID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}