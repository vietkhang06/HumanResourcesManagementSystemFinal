using System.Windows;
using HumanResourcesManagementSystemFinal.Models;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class EmployeeDetailWindow : Window
    {
        public EmployeeDetailWindow(Employee emp)
        {
            InitializeComponent();
            this.DataContext = emp;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}