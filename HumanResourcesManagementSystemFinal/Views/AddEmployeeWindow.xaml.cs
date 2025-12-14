using System.Windows;
using System.Windows.Input;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.ViewModels;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddEmployeeWindow : Window
    {
        public AddEmployeeWindow()
        {
            InitializeComponent();
            // Mặc định ViewModel đã khởi tạo ở chế độ Add
        }

        // Constructor cho chế độ Edit
        public AddEmployeeWindow(Employee empToEdit)
        {
            InitializeComponent();

            // Lấy ViewModel từ DataContext (đã được khai báo trong XAML)
            if (this.DataContext is AddEmployeeViewModel vm)
            {
                vm.LoadEmployeeForEdit(empToEdit);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}