using HumanResourcesManagementSystemFinal.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Cần thiết để dùng LINQ
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddEmployeeWindow : Window
    {
        public Employee NewEmployee { get; private set; }
        public string SelectedImagePath { get; private set; }
        public decimal BaseSalary { get; private set; }
        public DateTime StartDate { get; private set; }

        private bool _isEditMode = false;
        private int _editingId = 0;
        private IEnumerable<Employee> _managers; // Biến lưu danh sách quản lý

        // --- CONSTRUCTOR 1: THÊM MỚI (2 Tham số) ---
        public AddEmployeeWindow(IEnumerable<Department> departments, IEnumerable<Employee> managers)
        {
            InitializeComponent();
            cboDepartment.ItemsSource = departments;

            // Setup danh sách quản lý
            _managers = managers;
            cboManager.ItemsSource = _managers;

            this.Title = "Thêm Nhân Viên Mới";
        }

        // --- CONSTRUCTOR 2: SỬA (3 Tham số - Đây là cái bạn đang thiếu) ---
        public AddEmployeeWindow(IEnumerable<Department> departments, IEnumerable<Employee> managers, Employee empToEdit)
            : this(departments, managers) // Gọi lại constructor trên để nạp Dept/Managers trước
        {
            _isEditMode = true;
            _editingId = empToEdit.Id; // Hoặc EmployeeId tùy Model
            this.Title = "Cập Nhật Hồ Sơ";

            // 1. Điền dữ liệu cũ vào Form
            txtFirstName.Text = empToEdit.FirstName;
            txtLastName.Text = empToEdit.LastName;
            txtEmail.Text = empToEdit.Email;
            txtPhone.Text = empToEdit.PhoneNumber;
            txtAddress.Text = empToEdit.Address;
            txtPosition.Text = empToEdit.Position != null ? empToEdit.Position.Title : "";

            // 2. Chọn đúng Phòng ban cũ
            if (empToEdit.DepartmentId != null)
            {
                foreach (Department dept in cboDepartment.ItemsSource)
                {
                    if (dept.Id == empToEdit.DepartmentId) // Hoặc DepartmentId
                    {
                        cboDepartment.SelectedItem = dept;
                        break;
                    }
                }
            }

            // 3. Chọn đúng Quản lý cũ (Lọc bỏ chính mình để không tự quản lý mình)
            if (_managers != null)
            {
                // Loại bỏ chính nhân viên đang sửa khỏi danh sách sếp
                var validManagers = _managers.Where(m => m.Id != empToEdit.Id).ToList();
                cboManager.ItemsSource = validManagers;

                if (empToEdit.ManagerId != null)
                {
                    foreach (Employee m in cboManager.ItemsSource)
                    {
                        if (m.Id == empToEdit.ManagerId)
                        {
                            cboManager.SelectedItem = m;
                            break;
                        }
                    }
                }
            }

            // 4. Load Ảnh cũ
            LoadExistingImage(_editingId);
        }

        // --- HÀM LOAD ẢNH ---
        private void LoadExistingImage(int empId)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                string pngPath = Path.Combine(folder, $"{empId}.png");

                if (File.Exists(pngPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(pngPath);
                    bitmap.EndInit();
                    imgAvatarPreview.ImageSource = bitmap;
                }
            }
            catch { }
        }

        // --- SỰ KIỆN LƯU ---
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || cboDepartment.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng nhập Tên và chọn Phòng ban!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewEmployee = new Employee
            {
                Id = _isEditMode ? _editingId : 0, // Dùng Id hoặc EmployeeId
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Email = txtEmail.Text,
                PhoneNumber = txtPhone.Text,
                Address = txtAddress.Text,
                IsActive = true,

                DepartmentId = (cboDepartment.SelectedItem as Department).Id, // Hoặc DepartmentId
                Department = cboDepartment.SelectedItem as Department,

                // Lấy Manager ID
                ManagerId = (cboManager.SelectedItem as Employee)?.Id,
                Manager = cboManager.SelectedItem as Employee,

                Position = new Position { Title = txtPosition.Text }
            };

            // Lấy Lương & Ngày
            if (decimal.TryParse(txtBaseSalary.Text, out decimal salary)) BaseSalary = salary;
            else BaseSalary = 0;

            StartDate = dpStartDate.SelectedDate ?? DateTime.Now;

            DialogResult = true;
            this.Close();
        }

        // --- CÁC SỰ KIỆN KHÁC (Giữ nguyên) ---
        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                SelectedImagePath = dlg.FileName;
                imgAvatarPreview.ImageSource = new BitmapImage(new Uri(SelectedImagePath));
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}