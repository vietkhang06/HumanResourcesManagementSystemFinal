using HumanResourcesManagementSystemFinal.Data;
using System.Windows;

namespace HumanResourcesManagementSystemFinal
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Khởi tạo Database và Seed data
            using (var context = new DataContext())
            {
                DbSeeder.Seed(context);
            }
        }
    }
}