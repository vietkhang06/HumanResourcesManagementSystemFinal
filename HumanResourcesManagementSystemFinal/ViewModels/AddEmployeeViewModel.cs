using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class AddEmployeeViewModel : ObservableObject
    {
        private Employee _editingEmployee;
        private string _fullName;
        private string _cccd;
        private string _email;
        private string _phone;
        private string _address;
        private DateTime _birthDate = DateTime.Now.AddYears(-22);
        private string _gender = "Male";

        // Thay vì lưu đường dẫn, ta lưu mảng byte tạm thời
        private byte[] _selectedImageBytes = null;

        public bool IsEditMode => _editingEmployee != null;
        public string EditingEmployeeId => _editingEmployee?.EmployeeID;

        public ObservableCollection<Department> Departments { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();
        public ObservableCollection<Role> Roles { get; set; } = new();
        public ObservableCollection<Employee> Managers { get; set; } = new();

        [ObservableProperty] private string _windowTitle = "THÊM NHÂN VIÊN MỚI";
        [ObservableProperty] private ImageSource _avatarSource;
        [ObservableProperty] private Department _selectedDepartment;
        [ObservableProperty] private Position _selectedPosition;
        [ObservableProperty] private Employee _selectedManager;
        [ObservableProperty] private string _contractType = "Full-time";
        [ObservableProperty] private string _salaryString;
        [ObservableProperty] private DateTime _startDate = DateTime.Now;
        [ObservableProperty] private DateTime _endDate = DateTime.Now.AddYears(1);
        [ObservableProperty] private string _username;
        [ObservableProperty] private Role _selectedRole;

        public string FullName { get => _fullName; set { _fullName = string.IsNullOrWhiteSpace(value) ? null : value; OnPropertyChanged(); } }
        public string CCCD { get => _cccd; set => SetProperty(ref _cccd, value); }
        public string Email { get => _email; set { _email = string.IsNullOrWhiteSpace(value) ? null : value; OnPropertyChanged(); } }
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
        public string Address { get => _address; set => SetProperty(ref _address, value); }
        public DateTime BirthDate { get => _birthDate; set => SetProperty(ref _birthDate, value); }
        public string Gender { get => _gender; set => SetProperty(ref _gender, value); }

        public AddEmployeeViewModel()
        {
            LoadComboBoxData();
            // Load ảnh mặc định từ Resource
            AvatarSource = LoadImageFromBytes(null);
        }

        public AddEmployeeViewModel(Employee emp) : this()
        {
            if (emp != null) LoadEmployeeForEdit(emp);
        }

        // Hàm chuyển Byte[] sang ImageSource
        private ImageSource LoadImageFromBytes(byte[] imageData)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    using (var ms = new MemoryStream(imageData))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                }
            }
            catch { }
            return new BitmapImage(new Uri("pack://application:,,,/Images/default_user.png"));
        }

        [RelayCommand]
        private void UploadImage()
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // Đọc file thành byte ngay lập tức
                    _selectedImageBytes = File.ReadAllBytes(dlg.FileName);
                    AvatarSource = LoadImageFromBytes(_selectedImageBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file ảnh: " + ex.Message);
                }
            }
        }

        [RelayCommand]
        private void Save(object parameter)
        {
            var values = (object[])parameter;
            var window = (Window)values[0];
            var pBox = (PasswordBox)values[1];
            var pBoxConfirm = (PasswordBox)values[2];

            if (string.IsNullOrWhiteSpace(FullName)) { MessageBox.Show("Vui lòng nhập Họ và Tên."); return; }
            if (SelectedDepartment == null || SelectedPosition == null) { MessageBox.Show("Vui lòng chọn Phòng ban và Chức vụ."); return; }
            if (!decimal.TryParse(SalaryString, out decimal salary)) { MessageBox.Show("Lương cơ bản phải là số."); return; }
            if (!IsEditMode && string.IsNullOrWhiteSpace(Username)) { MessageBox.Show("Vui lòng nhập Tên đăng nhập."); return; }

            if (!IsEditMode || !string.IsNullOrEmpty(pBox.Password))
            {
                if (string.IsNullOrEmpty(pBox.Password)) { MessageBox.Show("Vui lòng nhập mật khẩu."); return; }
                if (pBox.Password != pBoxConfirm.Password) { MessageBox.Show("Mật khẩu xác nhận không khớp!"); return; }
            }

            using var context = new DataContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                Employee emp;
                string action;
                string recordId;
                string details;
                string currentUserId = UserSession.CurrentEmployeeId ?? "ADMIN";

                if (IsEditMode)
                {
                    emp = context.Employees
                        .Include(e => e.Account).Include(e => e.WorkContracts)
                        .FirstOrDefault(e => e.EmployeeID == _editingEmployee.EmployeeID);

                    if (emp == null) return;

                    UpdateEmployeeInfo(emp);

                    // Cập nhật ảnh nếu có thay đổi
                    if (_selectedImageBytes != null)
                    {
                        if (emp.Account != null) emp.Account.AvatarData = _selectedImageBytes;
                    }

                    var contract = emp.WorkContracts.OrderByDescending(c => c.StartDate).FirstOrDefault();
                    if (contract == null)
                    {
                        contract = new WorkContract { ContractID = "HD" + emp.EmployeeID, EmployeeID = emp.EmployeeID };
                        context.WorkContracts.Add(contract);
                    }
                    UpdateContractInfo(contract, salary);

                    if (emp.Account != null)
                    {
                        emp.Account.RoleID = SelectedRole?.RoleID ?? emp.Account.RoleID;
                        if (!string.IsNullOrEmpty(pBox.Password)) emp.Account.Password = pBox.Password;
                    }

                    action = "UPDATE";
                    recordId = emp.EmployeeID;
                    details = $"Cập nhật nhân viên: {emp.FullName}";
                }
                else
                {
                    if (context.Accounts.Any(a => a.UserName == Username)) { MessageBox.Show("Tên đăng nhập này đã tồn tại."); return; }

                    string newId = GenerateNewEmployeeID(context);

                    emp = new Employee { EmployeeID = newId, Status = "Active" };
                    UpdateEmployeeInfo(emp);
                    context.Employees.Add(emp);

                    var contract = new WorkContract { ContractID = "HD" + newId, EmployeeID = newId };
                    UpdateContractInfo(contract, salary);
                    context.WorkContracts.Add(contract);

                    var newAccount = new Account
                    {
                        UserID = "TK" + newId,
                        EmployeeID = newId,
                        UserName = Username,
                        Password = pBox.Password,
                        RoleID = SelectedRole?.RoleID,
                        IsActive = "Active",
                        AvatarData = _selectedImageBytes // Lưu ảnh vào Account mới
                    };
                    context.Accounts.Add(newAccount);

                    action = "CREATE";
                    recordId = newId;
                    details = $"Thêm mới nhân viên: {emp.FullName}";
                }

                context.ChangeHistories.Add(new ChangeHistory { LogID = Guid.NewGuid().ToString("N")[..8].ToUpper(), TableName = "Employees", ActionType = action, RecordID = recordId, ChangeByUserID = currentUserId, ChangeTime = DateTime.Now, Details = details });

                context.SaveChanges();
                transaction.Commit();

                MessageBox.Show("Lưu hồ sơ thành công!");
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
            }
        }

        [RelayCommand]
        private void Cancel(Window window) => window?.Close();

        private void LoadComboBoxData()
        {
            using var context = new DataContext();
            Departments = new(context.Departments.ToList());
            Positions = new(context.Positions.ToList());
            Roles = new(context.Roles.ToList());
            Managers = new(context.Employees.ToList());

            if (!IsEditMode) SelectedRole = Roles.FirstOrDefault(r => r.RoleID == "R002");
        }

        private void LoadEmployeeForEdit(Employee emp)
        {
            _editingEmployee = emp;
            WindowTitle = "CẬP NHẬT THÔNG TIN";
            OnPropertyChanged(nameof(IsEditMode));

            FullName = emp.FullName;
            CCCD = emp.CCCD;
            Email = emp.Email;
            Phone = emp.PhoneNumber;
            Address = emp.Address;
            BirthDate = emp.DateOfBirth ?? BirthDate;
            Gender = emp.Gender;

            // Load ảnh từ Database (thông qua Account)
            using var context = new DataContext();
            var fullEmp = context.Employees.Include(e => e.WorkContracts).Include(e => e.Account).FirstOrDefault(e => e.EmployeeID == emp.EmployeeID);

            if (fullEmp?.Account?.AvatarData != null)
            {
                AvatarSource = LoadImageFromBytes(fullEmp.Account.AvatarData);
                // Giữ lại byte cũ phòng trường hợp user không đổi ảnh
                _selectedImageBytes = fullEmp.Account.AvatarData;
            }
            else
            {
                AvatarSource = LoadImageFromBytes(null);
            }

            SelectedDepartment = Departments.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);
            SelectedPosition = Positions.FirstOrDefault(p => p.PositionID == emp.PositionID);
            SelectedManager = Managers.FirstOrDefault(m => m.EmployeeID == emp.ManagerID);

            if (fullEmp?.WorkContracts.Any() == true)
            {
                var c = fullEmp.WorkContracts.OrderByDescending(x => x.StartDate).First();
                ContractType = c.ContractType;
                SalaryString = c.Salary?.ToString("F0");
                StartDate = c.StartDate ?? StartDate;
                EndDate = c.EndDate ?? EndDate;
            }
            if (fullEmp?.Account != null)
            {
                Username = fullEmp.Account.UserName;
                SelectedRole = Roles.FirstOrDefault(r => r.RoleID == fullEmp.Account.RoleID);
            }
        }

        private void UpdateEmployeeInfo(Employee e)
        {
            e.FullName = FullName;
            e.CCCD = CCCD;
            e.Email = Email;
            e.PhoneNumber = Phone;
            e.Address = Address;
            e.DateOfBirth = BirthDate;
            e.Gender = Gender;
            e.DepartmentID = SelectedDepartment?.DepartmentID;
            e.PositionID = SelectedPosition?.PositionID;
            e.ManagerID = SelectedManager?.EmployeeID;
        }

        private void UpdateContractInfo(WorkContract c, decimal salary)
        {
            c.Salary = salary;
            c.ContractType = ContractType;
            c.StartDate = StartDate;
            c.EndDate = EndDate;
        }

        private string GenerateNewEmployeeID(DataContext context)
        {
            var lastId = context.Employees.Where(e => e.EmployeeID.StartsWith("NV")).OrderByDescending(e => e.EmployeeID).Select(e => e.EmployeeID).FirstOrDefault();
            if (string.IsNullOrEmpty(lastId)) return "NV001";
            return int.TryParse(lastId[2..], out int n) ? $"NV{n + 1:D3}" : $"NV{DateTime.Now.Ticks % 1000:D3}";
        }
    }
}