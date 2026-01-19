using System.Windows;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.ViewModels;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddEmployeeWindow : Window
    {
        // 1. Dùng cho nút THÊM MỚI
        public AddEmployeeWindow()
        {
            InitializeComponent();
            this.DataContext = new AddEmployeeViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddEmployeeViewModel vm)
            {
                // If we are in Edit Mode (modifying an existing employee)
                if (vm.IsEditMode)
                {
                    // Set a dummy string of 5 characters. 
                    // Because it's a PasswordBox, these will appear as dots (●●●●●).
                    pBox.Password = ".....";
                    pBoxConfirm.Password = ".....";
                }
            }
        }

        // 2. Dùng cho nút SỬA (Quan trọng!)
        public AddEmployeeWindow(Employee employeeEdit)
        {
            InitializeComponent();
            // Truyền nhân viên cần sửa vào ViewModel
            this.DataContext = new AddEmployeeViewModel(employeeEdit);
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}