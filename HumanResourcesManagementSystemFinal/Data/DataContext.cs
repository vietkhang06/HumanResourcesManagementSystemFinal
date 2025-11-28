using Microsoft.EntityFrameworkCore;
using HumanResourcesManagementSystemFinal.Models; // Nhớ using Models

namespace HumanResourcesManagementSystemFinal.Data
{
    public class DataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=HRMS_Pro.db");
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin"},
                new Role { RoleId = 2, RoleName = "Employee"}
            );

            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    AccountId = 1,
                    Username = "admin",
                    PasswordHash = "123", 
                    IsActive = true,
                    RoleId = 1,
                    EmployeeId = null 
                }
            );
        }
    }
}