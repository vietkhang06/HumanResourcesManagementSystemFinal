using HumanResourcesManagementSystemFinal.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddEmployeeWindow : Window
    {
        public AddEmployeeWindow()
        {
            InitializeComponent();
        }
        public Employee NewEmployee { get; private set; }
        public string SelectedImagePath { get; private set; }
        public decimal BaseSalary { get; private set; }
        public DateTime StartDate { get; private set; }

        private bool _isEditMode = false;
        private int _editingId = 0;
        private IEnumerable<Employee> _managers;

        private readonly Dictionary<string, List<string>> _positionsByDept = new Dictionary<string, List<string>>()
        {
            { "Phòng IT", new List<string> { "IT Support", "Junior Developer", "Senior Developer", "Tester", "Project Manager", "IT Director" } },
            { "Phòng Nhân Sự", new List<string> { "Thực tập sinh HR", "Chuyên viên tuyển dụng", "C&B Specialist", "Trưởng phòng Nhân sự" } },
            { "Phòng Kinh Doanh", new List<string> { "Sales Admin", "Nhân viên Kinh doanh", "Team Leader", "Giám đốc Kinh doanh" } },
            { "Phòng Kế Toán", new List<string> { "Kế toán viên", "Kế toán thuế", "Kế toán trưởng", "Thủ quỹ" } },
            { "Phòng Marketing", new List<string> { "Content Creator", "Designer", "SEO Specialist", "Marketing Manager" } }
        };

        public AddEmployeeWindow(IEnumerable<Department> departments, IEnumerable<Employee> managers)
        {
            InitializeComponent();
            cboDepartment.ItemsSource = departments;

            _managers = managers;
            cboManager.ItemsSource = _managers;

            this.Title = "Thêm Nhân Viên Mới";

            cboDepartment.SelectionChanged += CboDepartment_SelectionChanged;
        }

        public AddEmployeeWindow(IEnumerable<Department> departments, IEnumerable<Employee> managers, Employee empToEdit)
            : this(departments, managers)
        {
            dpStartDate.SelectedDate = DateTime.Now;
            _isEditMode = true;
            _editingId = empToEdit.Id;
            this.Title = "Cập Nhật Hồ Sơ";

            txtFirstName.Text = empToEdit.FirstName;
            txtLastName.Text = empToEdit.LastName;
            txtEmail.Text = empToEdit.Email;
            txtPhone.Text = empToEdit.PhoneNumber;
            txtAddress.Text = empToEdit.Address;

            if (empToEdit.DepartmentId != null)
            {
                foreach (Department dept in cboDepartment.ItemsSource)
                {
                    if (dept.Id == empToEdit.DepartmentId)
                    {
                        cboDepartment.SelectedItem = dept;
                        break;
                    }
                }
            }

            if (empToEdit.Position != null)
            {
                cboPosition.SelectedItem = empToEdit.Position.Title;
            }

            if (_managers != null)
            {
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

            LoadExistingImage(_editingId);
        }

        private void CboDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboDepartment.SelectedItem is Department selectedDept)
            {
                cboPosition.IsEnabled = true;
                cboPosition.ItemsSource = null;

                if (_positionsByDept.ContainsKey(selectedDept.DepartmentName))
                {
                    cboPosition.ItemsSource = _positionsByDept[selectedDept.DepartmentName];
                }
                else
                {
                    cboPosition.ItemsSource = new List<string> { "Nhân viên", "Trưởng nhóm", "Trưởng phòng", "Thực tập sinh" };
                }
            }
        }

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || cboDepartment.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng nhập Tên và chọn Phòng ban!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cboPosition.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn chức vụ!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewEmployee = new Employee
            {
                Id = _isEditMode ? _editingId : 0,
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Email = txtEmail.Text,
                PhoneNumber = txtPhone.Text,
                Address = txtAddress.Text,
                IsActive = true,

                DepartmentId = (cboDepartment.SelectedItem as Department).Id,
                Department = cboDepartment.SelectedItem as Department,

                ManagerId = (cboManager.SelectedItem as Employee)?.Id,
                Manager = cboManager.SelectedItem as Employee,

                Position = new Position { Title = cboPosition.SelectedItem.ToString() }
            };

            if (decimal.TryParse(txtBaseSalary.Text, out decimal salary)) BaseSalary = salary;
            else BaseSalary = 0;

            StartDate = dpStartDate.SelectedDate ?? DateTime.Now;

            DialogResult = true;
            this.Close();
        }

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