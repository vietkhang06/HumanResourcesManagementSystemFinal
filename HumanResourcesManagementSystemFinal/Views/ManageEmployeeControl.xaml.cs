using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HumanResourcesManagementSystemFinal.Views
{
    /// <summary>
    /// Interaction logic for ManageEmployeeControl.xaml
    /// </summary>
    public partial class ManageEmployeeControl : UserControl
    {
        public ManageEmployeeControl()
        {
            InitializeComponent();
        }
        private void BtnEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var emp = button.DataContext as Employee;

            if (emp != null)
            {
                var editWindow = new AddEmployeeWindow(emp);
                if (editWindow.ShowDialog() == true)
                {
                    if (DataContext is ManageEmployeeViewModel vm)
                    {
                        vm.LoadDataFromDb();
                    }
                }
            }
        }
    }
}
