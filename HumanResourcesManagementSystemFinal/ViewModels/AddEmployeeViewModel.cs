using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class AddEmployeeViewModel : ObservableObject
    {
        // === BIẾN TRẠNG THÁI ===
        private Employee _editingEmployee = null;
        public bool IsEditMode => _editingEmployee != null;

        // === 1. DỮ LIỆU COMBOBOX ===
        public ObservableCollection<Department> Departments { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();
        public ObservableCollection<Role> Roles { get; set; } = new();
        public ObservableCollection<Employee> Managers { get; set; } = new();

        // === 2. DỮ LIỆU NHẬP LIỆU ===
        [ObservableProperty] private string _windowTitle = "THÊM NHÂN VIÊN MỚI";
        [ObservableProperty] private string _firstName;
        [ObservableProperty] private string _lastName;
        [ObservableProperty] private string _email;
        [ObservableProperty] private string _phone;
        [ObservableProperty] private string _address;
        [ObservableProperty] private DateTime _birthDate = DateTime.Now.AddYears(-22);
        [ObservableProperty] private string _gender = "Male";
        [ObservableProperty] private string _avatarSource = "/Images/png2.png";
        private string _selectedImagePath;

        [ObservableProperty] private Department _selectedDepartment;
        [ObservableProperty] private Position _selectedPosition;
        [ObservableProperty] private Employee _selectedManager;

        // Hợp đồng
        [ObservableProperty] private string _contractType = "Full-time";
        [ObservableProperty] private string _salaryString;
        [ObservableProperty] private DateTime _startDate = DateTime.Now;
        [ObservableProperty] private DateTime _endDate = DateTime.Now.AddYears(1);

        // Tài khoản
        [ObservableProperty] private string _username;
        [ObservableProperty] private Role _selectedRole;

        // ID Admin đang đăng nhập (Lấy từ Session)
        private int _currentAdminId = UserSession.CurrentEmployeeId != 0 ? UserSession.CurrentEmployeeId : 1;

        // --- CONSTRUCTOR ---
        public AddEmployeeViewModel()
        {
            LoadComboBoxData();
        }

        // --- HÀM LOAD DỮ LIỆU ĐỂ SỬA ---
        public void LoadEmployeeForEdit(Employee emp)
        {
            _editingEmployee = emp;
            WindowTitle = "CẬP NHẬT THÔNG TIN NHÂN VIÊN";
            OnPropertyChanged(nameof(IsEditMode));

            FirstName = emp.FirstName;
            LastName = emp.LastName;
            Email = emp.Email;
            Phone = emp.PhoneNumber;
            Address = emp.Address;
            BirthDate = emp.DateOfBirth ?? DateTime.Now;
            Gender = emp.Gender;

            string imagePath = GetImagePath(emp.Id);
            if (!string.IsNullOrEmpty(imagePath)) AvatarSource = imagePath;

            SelectedDepartment = Departments.FirstOrDefault(d => d.Id == emp.DepartmentId);
            SelectedPosition = Positions.FirstOrDefault(p => p.Id == emp.PositionId);
            SelectedManager = Managers.FirstOrDefault(m => m.Id == emp.ManagerId);

            using (var context = new DataContext())
            {
                var fullEmp = context.Employees
                    .Include(e => e.WorkContracts)
                    .Include(e => e.Account)
                    .FirstOrDefault(e => e.Id == emp.Id);

                if (fullEmp != null)
                {
                    var contract = fullEmp.WorkContracts.OrderByDescending(c => c.StartDate).FirstOrDefault();
                    if (contract != null)
                    {
                        ContractType = contract.ContractType;
                        SalaryString = contract.Salary.ToString("F0");
                        StartDate = contract.StartDate;
                        EndDate = contract.EndDate ?? DateTime.Now.AddYears(1);
                    }

                    if (fullEmp.Account != null)
                    {
                        Username = fullEmp.Account.Username;
                        SelectedRole = Roles.FirstOrDefault(r => r.RoleId == fullEmp.Account.RoleId);
                    }
                }
            }
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
                    Managers = new ObservableCollection<Employee>(context.Employees.ToList());

                    // Set mặc định Role
                    var defaultRole = Roles.FirstOrDefault(r =>
                        r.RoleName.ToLower().Contains("employee") ||
                        r.RoleName.ToLower().Contains("staff") ||
                        r.RoleName.ToLower().Contains("nhân viên"));

                    if (defaultRole != null) SelectedRole = defaultRole;
                }
            }
            catch { }
        }

        [RelayCommand]
        private void UploadImage()
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                _selectedImagePath = dlg.FileName;
                AvatarSource = _selectedImagePath;
            }
        }

        [RelayCommand]
        private void Save(object parameter)
        {
            var values = (object[])parameter;
            var window = (Window)values[0];
            var pBox = (PasswordBox)values[1];
            var pBoxConfirm = (PasswordBox)values[2];

            // 1. VALIDATION CƠ BẢN
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Vui lòng nhập Họ và Tên.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate Phòng ban & Chức vụ (Để tránh lỗi Foreign Key)
            if (SelectedDepartment == null)
            {
                MessageBox.Show("Vui lòng chọn Phòng ban.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedPosition == null)
            {
                MessageBox.Show("Vui lòng chọn Chức vụ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(SalaryString, out decimal salary))
            {
                MessageBox.Show("Lương cơ bản phải là số.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsEditMode)
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(pBox.Password))
                {
                    MessageBox.Show("Vui lòng nhập Tên đăng nhập và Mật khẩu cho nhân viên mới.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(pBox.Password) && pBox.Password != pBoxConfirm.Password)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. XỬ LÝ DATABASE
            using (var context = new DataContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        Employee empToSave;
                        string actionType = "";
                        string logDetails = "";

                        // --- TRƯỜNG HỢP SỬA (UPDATE) ---
                        if (IsEditMode)
                        {
                            actionType = "UPDATE";
                            empToSave = context.Employees
                                .Include(e => e.Account)
                                .Include(e => e.WorkContracts)
                                .FirstOrDefault(e => e.Id == _editingEmployee.Id);

                            if (empToSave == null)
                            {
                                MessageBox.Show("Không tìm thấy nhân viên này trong CSDL.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Cập nhật thông tin
                            empToSave.FirstName = FirstName;
                            empToSave.LastName = LastName;
                            empToSave.Email = Email;
                            empToSave.PhoneNumber = Phone;
                            empToSave.Address = Address;
                            empToSave.DateOfBirth = BirthDate;
                            empToSave.Gender = Gender;
                            empToSave.DepartmentId = SelectedDepartment.Id;
                            empToSave.PositionId = SelectedPosition.Id;
                            empToSave.ManagerId = SelectedManager?.Id; // Có thể null
                            empToSave.HireDate = StartDate;

                            // Cập nhật Hợp đồng
                            var contract = empToSave.WorkContracts.LastOrDefault();
                            if (contract != null)
                            {
                                contract.Salary = salary;
                                contract.ContractType = ContractType;
                                contract.StartDate = StartDate;
                                contract.EndDate = EndDate;
                            }
                            else
                            {
                                context.WorkContracts.Add(new WorkContract
                                {
                                    EmployeeId = empToSave.Id,
                                    Salary = salary,
                                    ContractType = ContractType,
                                    StartDate = StartDate,
                                    EndDate = EndDate
                                });
                            }

                            // Cập nhật Tài khoản
                            if (empToSave.Account != null)
                            {
                                empToSave.Account.RoleId = SelectedRole?.RoleId ?? empToSave.Account.RoleId;
                                if (!string.IsNullOrEmpty(pBox.Password))
                                {
                                    empToSave.Account.PasswordHash = pBox.Password; // Nên mã hóa
                                }
                            }
                            // Nếu trước đó chưa có TK thì tạo mới (Optional logic)

                            logDetails = $"Cập nhật nhân viên: {empToSave.FullName}";
                        }
                        // --- TRƯỜNG HỢP THÊM MỚI (CREATE) ---
                        else
                        {
                            actionType = "CREATE";

                            // Kiểm tra trùng Username TRƯỚC KHI LƯU (Để báo lỗi đẹp hơn)
                            if (context.Accounts.Any(a => a.Username == Username))
                            {
                                MessageBox.Show($"Tên đăng nhập '{Username}' đã tồn tại! Vui lòng chọn tên khác.", "Trùng lặp", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            empToSave = new Employee
                            {
                                FirstName = FirstName,
                                LastName = LastName,
                                Email = Email,
                                PhoneNumber = Phone,
                                Address = Address,
                                DateOfBirth = BirthDate,
                                Gender = Gender,
                                DepartmentId = SelectedDepartment.Id,
                                PositionId = SelectedPosition.Id,
                                ManagerId = SelectedManager?.Id,
                                HireDate = StartDate,
                                IsActive = true
                            };
                            context.Employees.Add(empToSave);
                            context.SaveChanges(); // Lưu lần 1 để lấy ID

                            // Thêm Hợp đồng
                            context.WorkContracts.Add(new WorkContract
                            {
                                EmployeeId = empToSave.Id,
                                ContractType = ContractType,
                                Salary = salary,
                                StartDate = StartDate,
                                EndDate = EndDate
                            });

                            // Thêm Tài khoản
                            if (SelectedRole == null)
                            {
                                SelectedRole = Roles.FirstOrDefault(r => r.RoleName == "Employee");
                            }

                            context.Accounts.Add(new Account
                            {
                                EmployeeId = empToSave.Id,
                                Username = Username,
                                PasswordHash = pBox.Password,
                                RoleId = SelectedRole?.RoleId ?? 1,
                                IsActive = true
                            });

                            logDetails = $"Thêm nhân viên mới: {LastName} {FirstName}, user: {Username}";
                        }

                        // === LƯU THAY ĐỔI VÀO DB ===
                        // Ghi Log Audit
                        AuditService.LogChange(context, "Employees", actionType, empToSave.Id, _currentAdminId, logDetails);

                        context.SaveChanges(); // Lưu tất cả thay đổi còn lại
                        transaction.Commit();

                        // === LƯU ẢNH (Chỉ lưu sau khi DB thành công) ===
                        if (!string.IsNullOrEmpty(_selectedImagePath))
                        {
                            SaveImageToFolder(empToSave.Id, _selectedImagePath);
                        }

                        MessageBox.Show(IsEditMode ? "Cập nhật thành công!" : "Thêm mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        window.DialogResult = true;
                        window.Close();
                    }
                    catch (DbUpdateException dbEx) // <--- BẮT LỖI CỤ THỂ CỦA DATABASE
                    {
                        transaction.Rollback();

                        var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                        // Kiểm tra lỗi trùng lặp (UNIQUE constraint)
                        if (innerMessage.Contains("UNIQUE") && innerMessage.Contains("Username"))
                        {
                            MessageBox.Show($"Tên đăng nhập '{Username}' đã được sử dụng bởi người khác.\nVui lòng chọn tên khác.", "Lỗi trùng lặp", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (innerMessage.Contains("FOREIGN KEY"))
                        {
                            MessageBox.Show("Lỗi liên kết dữ liệu (Phòng ban hoặc Chức vụ không hợp lệ).", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show("Lỗi Database chi tiết:\n" + innerMessage, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi không xác định: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        [RelayCommand]
        private void Cancel(Window window) => window?.Close();

        private void SaveImageToFolder(int empId, string sourcePath)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string dest = Path.Combine(folder, $"{empId}{Path.GetExtension(sourcePath)}");

                // Xóa ảnh cũ nếu có
                string[] exts = { ".png", ".jpg", ".jpeg" };
                foreach (var ext in exts)
                {
                    string oldPath = Path.Combine(folder, $"{empId}{ext}");
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                File.Copy(sourcePath, dest, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu ảnh: " + ex.Message);
            }
        }

        private string GetImagePath(int empId)
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
            string[] exts = { ".png", ".jpg", ".jpeg" };
            foreach (var ext in exts)
            {
                string path = Path.Combine(folder, $"{empId}{ext}");
                if (File.Exists(path)) return path;
            }
            return "";
        }
    }
}