using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HumanResourcesManagementSystemFinal.Data
{
    // Class này giúp EF Core tạo DB mà không cần chạy App WPF (Tránh lỗi STA Thread)
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            // Chỉ định lại file db giống y hệt trong DataContext.cs
            optionsBuilder.UseSqlite("Data Source=HRMS_Pro.db");

            return new DataContext();
        }
    }
}