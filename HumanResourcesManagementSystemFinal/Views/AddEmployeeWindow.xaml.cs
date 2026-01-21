using System.Windows;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.ViewModels;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddEmployeeWindow : Window
    {
        public AddEmployeeWindow()
        {
            InitializeComponent();
            this.DataContext = new AddEmployeeViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddEmployeeViewModel vm)
            {
                if (vm.IsEditMode)
                {
                    pBox.Password = ".....";
                    pBoxConfirm.Password = ".....";
                }
            }
        }

        public AddEmployeeWindow(Employee employeeEdit)
        {
            InitializeComponent();
            this.DataContext = new AddEmployeeViewModel(employeeEdit);
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}