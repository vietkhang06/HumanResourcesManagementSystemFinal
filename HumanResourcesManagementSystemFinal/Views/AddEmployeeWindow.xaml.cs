using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using HumanResourcesManagementSystemFinal.Models;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddEmployeeWindow : Window
    {
        // Property này dùng để trả kết quả về cho ViewModel
        public Employee NewEmployee { get; private set; }

        // Constructor nhận vào danh sách phòng ban để đổ vào ComboBox
        public AddEmployeeWindow(IEnumerable<Department> departments)
        {
            InitializeComponent();

            // Đổ dữ liệu phòng ban vào ComboBox
            cboDepartment.ItemsSource = departments;
        }

        // Sự kiện nút Lưu
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate cơ bản (Bắt buộc nhập Tên và Email)
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên và Email!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Tạo đối tượng Employee mới từ giao diện
            NewEmployee = new Employee
            {
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Email = txtEmail.Text,
                PhoneNumber = txtPhone.Text,
                Address = txtAddress.Text,

                // Lấy phòng ban đã chọn
                Department = cboDepartment.SelectedItem as Department,
                DepartmentId = (cboDepartment.SelectedItem as Department)?.Id ?? 0,

                // Giả sử Position là string, nếu là Object thì cần xử lý tương tự Department
                Position = new Position { Title = txtPosition.Text }
            };

            // 3. Đóng form và báo thành công (DialogResult = true)
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        // Cho phép kéo thả cửa sổ (vì WindowStyle=None nên mất thanh tiêu đề mặc định)
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}