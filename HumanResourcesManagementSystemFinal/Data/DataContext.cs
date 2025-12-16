using System;
using Microsoft.EntityFrameworkCore;
using HumanResourcesManagementSystemFinal.Models;

namespace HumanResourcesManagementSystemFinal.Data
{
    public class DataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = System.IO.Path.Combine(path, "HRMS.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<WorkContract> WorkContracts { get; set; }
        public DbSet<TimeSheet> TimeSheets { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<ChangeHistory> ChangeHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Employee" }
            );

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, DepartmentName = "Ban Giám Đốc", Location = "Trụ sở chính" }
            );

            modelBuilder.Entity<Position>().HasData(
                new Position { Id = 1, Title = "Quản Trị Viên", JobDescription = "Admin hệ thống", DepartmentId = 1 }
            );

            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    FirstName = "System",
                    LastName = "Admin",
                    Gender = "Other",
                    HireDate = DateTime.Now,
                    IsActive = true,
                    DepartmentId = 1,
                    PositionId = 1
                }
            );

            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    AccountId = 1,
                    Username = "admin",
                    PasswordHash = "123",
                    IsActive = true,
                    RoleId = 1,
                    EmployeeId = 1
                }
            );
        }
    }
}