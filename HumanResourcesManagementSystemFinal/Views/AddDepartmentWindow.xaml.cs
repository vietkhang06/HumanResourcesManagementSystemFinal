using HumanResourcesManagementSystemFinal.Models;
using System.Windows;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddDepartmentWindow : Window
    {
        public string DeptName { get; private set; }
        public string DeptLocation { get; private set; }

        public AddDepartmentWindow()
        {
            InitializeComponent();
            txtDeptName.Focus();
        }
        public AddDepartmentWindow(Department deptToEdit = null)
        {
            InitializeComponent();

            if (deptToEdit != null)
            {
                Title = "Cập Nhật Phòng Ban";
                txtTitle.Text = "✏️ Chỉnh Sửa Phòng Ban";
                txtDeptName.Text = deptToEdit.DepartmentName;
                txtLocation.Text = deptToEdit.Location;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDeptName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng ban!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDeptName.Focus();
                return;
            }

            DeptName = txtDeptName.Text.Trim();
            DeptLocation = txtLocation.Text.Trim();

            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

    }
}