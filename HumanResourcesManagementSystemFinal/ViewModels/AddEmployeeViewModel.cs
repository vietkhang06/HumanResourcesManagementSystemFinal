using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls; // Để dùng PasswordBox

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class AddEmployeeViewModel : ObservableObject
    {
        // === 1. DỮ LIỆU ĐỂ BINDING CHO COMBOBOX ===
        public ObservableCollection<Department> Departments { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();
        public ObservableCollection<Role> Roles { get; set; } = new();
        public ObservableCollection<Employee> Managers { get; set; } = new(); // List người quản lý

        // === 2. DỮ LIỆU NHẬP LIỆU (TWO-WAY BINDING) ===
        // -- Thông tin cá nhân --
        [ObservableProperty] private string _firstName;
        [ObservableProperty] private string _lastName;
        [ObservableProperty] private string _email;
        [ObservableProperty] private string _phone;
        [ObservableProperty] private string _address;
        [ObservableProperty] private DateTime _birthDate = DateTime.Now.AddYears(-22);
        [ObservableProperty] private string _gender = "Male"; // Mặc định
        [ObservableProperty] private string _avatarSource = "/Images/png2.png"; // Ảnh mặc định
        private string _selectedImagePath; // Đường dẫn ảnh gốc để copy

        [ObservableProperty] private Department _selectedDepartment;
        [ObservableProperty] private Position _selectedPosition;
        [ObservableProperty] private Employee _selectedManager;

        // -- Hợp đồng --
        [ObservableProperty] private string _contractNumber; // Nếu cần
        [ObservableProperty] private string _contractType = "Full-time";
        [ObservableProperty] private string _salaryString; // Để string để dễ validate
        [ObservableProperty] private DateTime _startDate = DateTime.Now;
        [ObservableProperty] private DateTime _endDate = DateTime.Now.AddYears(1);

        // -- Tài khoản --
        [ObservableProperty] private string _username;
        [ObservableProperty] private Role _selectedRole;
        // Password không binding trực tiếp vì lý do bảo mật

        public AddEmployeeViewModel()
        {
            LoadComboBoxData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var context = new DataContext())
                {
                    Departments = new ObservableCollection<Department>(context.Departments.ToList());
                    Positions = new ObservableCollection<Position>(context.Positions.ToList());
                    Roles = new ObservableCollection<Role>(context.Roles.ToList());

                    // Load Manager: Lấy danh sách nhân viên để chọn làm quản lý
                    Managers = new ObservableCollection<Employee>(context.Employees.ToList());

                    // Nếu chưa có Role nào thì tạo mặc định (Optional)
                    if (Roles.Count == 0)
                    {
                        context.Roles.AddRange(new Role { RoleName = "Admin" }, new Role { RoleName = "Staff" });
                        context.SaveChanges();
                        Roles = new ObservableCollection<Role>(context.Roles.ToList());
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        [RelayCommand]
        private void UploadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg" };
            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                AvatarSource = _selectedImagePath; // Cập nhật hiển thị ngay lập tức
            }
        }

        // Command Lưu: Nhận vào tham số là Window (để đóng) và PasswordBox
        [RelayCommand]
        private void Save(object parameter)
        {
            // Lấy các tham số truyền từ View (Window, PasswordBox, ConfirmPasswordBox)
            var values = (object[])parameter;
            var window = (Window)values[0];
            var pBox = (PasswordBox)values[1];
            var pBoxConfirm = (PasswordBox)values[2];

            // 1. VALIDATION
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Họ và Tên.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(pBox.Password))
            {
                MessageBox.Show("Vui lòng nhập thông tin tài khoản.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (pBox.Password != pBoxConfirm.Password)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!decimal.TryParse(_salaryString, out decimal salary))
            {
                MessageBox.Show("Lương phải là số hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. LƯU DỮ LIỆU (TRANSACTION)
            using (var context = new DataContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // A. Tạo Employee
                        var newEmp = new Employee
                        {
                            FirstName = FirstName.Trim(),
                            LastName = LastName.Trim(),
                            Email = Email,
                            PhoneNumber = Phone,
                            Address = Address,
                            DateOfBirth = BirthDate,
                            Gender = Gender,
                            DepartmentId = SelectedDepartment?.Id, // nullable
                            PositionId = SelectedPosition?.Id,     // nullable
                            ManagerId = SelectedManager?.Id,               // nullable
                            HireDate = StartDate,
                            IsActive = true
                        };
                        context.Employees.Add(newEmp);
                        context.SaveChanges(); // Lưu để lấy ID

                        // B. Xử lý ảnh (Lưu sau khi có ID)
                        if (!string.IsNullOrEmpty(_selectedImagePath))
                        {
                            SaveImageToFolder(newEmp.Id, _selectedImagePath);
                        }

                        // C. Tạo Hợp Đồng
                        var contract = new WorkContract
                        {
                            EmployeeId = newEmp.Id,
                            ContractType = ContractType,
                            Salary = salary,
                            StartDate = StartDate,
                            EndDate = EndDate
                        };
                        context.WorkContracts.Add(contract);

                        // D. Tạo Tài Khoản
                        var account = new Account
                        {
                            EmployeeId = newEmp.Id,
                            Username = Username.Trim(),
                            PasswordHash = pBox.Password, // Lưu ý: Thực tế nên mã hóa (MD5/BCrypt)
                            RoleId = SelectedRole?.RoleId ?? (Roles.FirstOrDefault()?.RoleId ?? 1),
                            IsActive = true
                        };
                        context.Accounts.Add(account);

                        context.SaveChanges();
                        transaction.Commit(); // Xác nhận mọi thứ thành công

                        MessageBox.Show("Thêm mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Đóng cửa sổ và trả về DialogResult = true
                        window.DialogResult = true;
                        window.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Hoàn tác nếu lỗi
                        MessageBox.Show($"Lỗi lưu dữ liệu: {ex.Message}\n{ex.InnerException?.Message}", "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            window?.Close();
        }

        private void SaveImageToFolder(int empId, string sourcePath)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string ext = Path.GetExtension(sourcePath);
                string dest = Path.Combine(folder, $"{empId}{ext}");
                File.Copy(sourcePath, dest, true);
            }
            catch { /* Ignore image error */ }
        }
    }
}