using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class AddEmployeeViewModel : ObservableObject
    {
        private Employee _editingEmployee = null;
        public bool IsEditMode => _editingEmployee != null;

        public ObservableCollection<Department> Departments { get; set; } = new();
        public ObservableCollection<Position> Positions { get; set; } = new();
        public ObservableCollection<Role> Roles { get; set; } = new();
        public ObservableCollection<Employee> Managers { get; set; } = new();

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

        [ObservableProperty] private string _contractType = "Full-time";
        [ObservableProperty] private string _salaryString;
        [ObservableProperty] private DateTime _startDate = DateTime.Now;
        [ObservableProperty] private DateTime _endDate = DateTime.Now.AddYears(1);

        [ObservableProperty] private string _username;
        [ObservableProperty] private Role _selectedRole;

        private int _currentAdminId = UserSession.CurrentEmployeeId != 0 ? UserSession.CurrentEmployeeId : 1;

        public AddEmployeeViewModel()
        {
            LoadComboBoxData();
        }

        public void LoadEmployeeForEdit(Employee emp)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    var defaultRole = Roles.FirstOrDefault(r =>
                        r.RoleName.ToLower().Contains("employee") ||
                        r.RoleName.ToLower().Contains("staff") ||
                        r.RoleName.ToLower().Contains("nhân viên"));

                    if (defaultRole != null) SelectedRole = defaultRole;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách phòng ban/chức vụ: " + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Vui lòng nhập Họ và Tên.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

            using (var context = new DataContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        Employee empToSave;
                        string actionType = "";
                        string logDetails = "";

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

                            empToSave.FirstName = FirstName;
                            empToSave.LastName = LastName;
                            empToSave.Email = Email;
                            empToSave.PhoneNumber = Phone;
                            empToSave.Address = Address;
                            empToSave.DateOfBirth = BirthDate;
                            empToSave.Gender = Gender;
                            empToSave.DepartmentId = SelectedDepartment.Id;
                            empToSave.PositionId = SelectedPosition.Id;
                            empToSave.ManagerId = SelectedManager?.Id;
                            empToSave.HireDate = StartDate;

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

                            if (empToSave.Account != null)
                            {
                                empToSave.Account.RoleId = SelectedRole?.RoleId ?? empToSave.Account.RoleId;
                                if (!string.IsNullOrEmpty(pBox.Password))
                                {
                                    empToSave.Account.PasswordHash = pBox.Password;
                                }
                            }

                            logDetails = $"Cập nhật nhân viên: {empToSave.FullName}";
                        }
                        else
                        {
                            actionType = "CREATE";

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
                            context.SaveChanges();

                            context.WorkContracts.Add(new WorkContract
                            {
                                EmployeeId = empToSave.Id,
                                ContractType = ContractType,
                                Salary = salary,
                                StartDate = StartDate,
                                EndDate = EndDate
                            });

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

                        AuditService.LogChange(context, "Employees", actionType, empToSave.Id, _currentAdminId, logDetails);

                        context.SaveChanges();
                        transaction.Commit();

                        if (!string.IsNullOrEmpty(_selectedImagePath))
                        {
                            SaveImageToFolder(empToSave.Id, _selectedImagePath);
                        }

                        MessageBox.Show(IsEditMode ? "Cập nhật thành công!" : "Thêm mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        window.DialogResult = true;
                        window.Close();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại trang.", "Lỗi xung đột", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (DbUpdateException dbEx)
                    {
                        transaction.Rollback();

                        var sb = new StringBuilder();
                        var inner = dbEx.InnerException;
                        while (inner != null)
                        {
                            sb.AppendLine(inner.Message);
                            inner = inner.InnerException;
                        }
                        var innerMessage = sb.ToString();

                        if (innerMessage.Contains("UNIQUE") || innerMessage.Contains("Duplicate"))
                        {
                            MessageBox.Show($"Dữ liệu bị trùng lặp (Username, Email hoặc SĐT đã tồn tại).\nChi tiết: {innerMessage}", "Lỗi trùng lặp", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (innerMessage.Contains("FOREIGN KEY") || innerMessage.Contains("REFERENCE"))
                        {
                            MessageBox.Show("Dữ liệu tham chiếu không hợp lệ (Phòng ban hoặc Chức vụ có thể đã bị xóa).", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show("Lỗi Database: " + innerMessage, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (InvalidOperationException invEx)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi thao tác dữ liệu: " + invEx.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi không xác định: " + ex.Message, "Lỗi Nghiêm Trọng", MessageBoxButton.OK, MessageBoxImage.Error);
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

                string[] exts = { ".png", ".jpg", ".jpeg" };
                foreach (var ext in exts)
                {
                    string oldPath = Path.Combine(folder, $"{empId}{ext}");
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                File.Copy(sourcePath, dest, true);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Không có quyền ghi vào thư mục lưu ảnh. Vui lòng chạy ứng dụng với quyền Admin.", "Lỗi quyền truy cập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (PathTooLongException)
            {
                MessageBox.Show("Đường dẫn file ảnh quá dài.", "Lỗi đường dẫn", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ioEx)
            {
                MessageBox.Show("Lỗi khi lưu file ảnh (File đang mở hoặc ổ đĩa đầy): " + ioEx.Message, "Lỗi IO", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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