using Microsoft.EntityFrameworkCore;
using HumanResourcesManagementSystemFinal.Models; // Đảm bảo using namespace chứa các Model

namespace HumanResourcesManagementSystemFinal.Data
{
    // BẮT BUỘC phải kế thừa từ : DbContext
    public class DataContext : DbContext
    {
        // Khai báo các bảng dữ liệu (DbSet)
        // Lưu ý: Tên thuộc tính (ví dụ: Accounts) phải khớp với tên bạn gọi trong LoginViewModel
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Positions { get; set; }

        // Cấu hình kết nối Database SQLite
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Tên file database sẽ là HrmsFinal.db
            optionsBuilder.UseSqlite("Data Source=HrmsFinal.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình thêm nếu cần (ví dụ Username là duy nhất)
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Username)
                .IsUnique();
        }
    }
}