using HumanResourcesManagementSystemFinal.Data;
using System.Windows;
using OfficeOpenXml;


namespace HumanResourcesManagementSystemFinal
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ExcelPackage.License.SetNonCommercialPersonal("HRMS Project");

            using (var context = new DataContext())
            {
                DbSeeder.Seed(context);
            }
        }
    }
}