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

        // Các Property Binding
        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) _fullName = null;
                else _fullName = value;
                OnPropertyChanged();
            }
        }

        private string _cccd;
        public string CCCD { get => _cccd; set => SetProperty(ref _cccd, value); }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) _email = null;
                else _email = value;
                OnPropertyChanged();
            }
        }

        private string _phone;
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

        private string _address;
        public string Address { get => _address; set => SetProperty(ref _address, value); }

        private DateTime _birthDate = DateTime.Now.AddYears(-22);
        public DateTime BirthDate { get => _birthDate; set => SetProperty(ref _birthDate, value); }

        private string _gender = "Male";
        public string Gender { get => _gender; set => SetProperty(ref _gender, value); }

        [ObservableProperty] private string _avatarSource = "/Images/default_user.png";
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

        // --- CONSTRUCTOR 1: Dùng cho Thêm Mới ---
        public AddEmployeeViewModel()
        {
            LoadComboBoxData();
        }

        // --- CONSTRUCTOR 2: Dùng cho Chỉnh Sửa (QUAN TRỌNG) ---
        public AddEmployeeViewModel(Employee emp) : this() // Gọi this() để load combobox trước
        {
            if (emp != null)
            {
                LoadEmployeeForEdit(emp); // Đổ dữ liệu cũ vào form
            }
        }

        // Hàm đổ dữ liệu cũ vào các ô nhập liệu
        public void LoadEmployeeForEdit(Employee emp)
        {
            try
            {
                _editingEmployee = emp;
                WindowTitle = "CẬP NHẬT THÔNG TIN";
                OnPropertyChanged(nameof(IsEditMode));

                FullName = emp.FullName;
                CCCD = emp.CCCD;
                Email = emp.Email;
                Phone = emp.PhoneNumber;
                Address = emp.Address;
                BirthDate = emp.DateOfBirth ?? DateTime.Now;
                Gender = emp.Gender;

                string imagePath = GetImagePath(emp.EmployeeID);
                if (!string.IsNullOrEmpty(imagePath)) AvatarSource = imagePath;

                // Load chi tiết từ DB để lấy Account và Contract
                using (var context = new DataContext())
                {
                    // Map Department, Position, Manager từ List đã load
                    SelectedDepartment = Departments.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);
                    SelectedPosition = Positions.FirstOrDefault(p => p.PositionID == emp.PositionID);
                    SelectedManager = Managers.FirstOrDefault(m => m.EmployeeID == emp.ManagerID);

                    var fullEmp = context.Employees
                        .Include(e => e.WorkContracts)
                        .Include(e => e.Account)
                        .FirstOrDefault(e => e.EmployeeID == emp.EmployeeID);

                    if (fullEmp != null)
                    {
                        var contract = fullEmp.WorkContracts.OrderByDescending(c => c.StartDate).FirstOrDefault();
                        if (contract != null)
                        {
                            ContractType = contract.ContractType;
                            SalaryString = contract.Salary?.ToString("F0") ?? "0";
                            StartDate = contract.StartDate ?? DateTime.Now;
                            EndDate = contract.EndDate ?? DateTime.Now.AddYears(1);
                        }

                        if (fullEmp.Account != null)
                        {
                            Username = fullEmp.Account.UserName;
                            SelectedRole = Roles.FirstOrDefault(r => r.RoleID == fullEmp.Account.RoleID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
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

                    if (!IsEditMode)
                    {
                        var defaultRole = Roles.FirstOrDefault(r => r.RoleID == "R002" || r.RoleName.Contains("Employee"));
                        if (defaultRole != null) SelectedRole = defaultRole;
                    }
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

            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(FullName)) { MessageBox.Show("Vui lòng nhập Họ và Tên."); return; }
            if (SelectedDepartment == null || SelectedPosition == null) { MessageBox.Show("Vui lòng chọn Phòng ban và Chức vụ."); return; }
            if (!decimal.TryParse(SalaryString, out decimal salary)) { MessageBox.Show("Lương cơ bản phải là số."); return; }

            if (!IsEditMode && string.IsNullOrWhiteSpace(Username)) { MessageBox.Show("Vui lòng nhập Tên đăng nhập."); return; }

            if (!IsEditMode || (IsEditMode && !string.IsNullOrEmpty(pBox.Password)))
            {
                if (string.IsNullOrEmpty(pBox.Password)) { MessageBox.Show("Vui lòng nhập mật khẩu."); return; }
                if (pBox.Password != pBoxConfirm.Password) { MessageBox.Show("Mật khẩu xác nhận không khớp!"); return; }
            }

            using (var context = new DataContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        Employee empToSave;

                        if (IsEditMode)
                        {
                            // UPDATE
                            empToSave = context.Employees
                                .Include(e => e.Account).Include(e => e.WorkContracts)
                                .FirstOrDefault(e => e.EmployeeID == _editingEmployee.EmployeeID);

                            if (empToSave == null) return;

                            UpdateEmployeeInfo(empToSave);

                            // Update Contract
                            var contract = empToSave.WorkContracts.OrderByDescending(c => c.StartDate).FirstOrDefault();
                            if (contract == null)
                            {
                                contract = new WorkContract { ContractID = "HD" + empToSave.EmployeeID, EmployeeID = empToSave.EmployeeID };
                                context.WorkContracts.Add(contract);
                            }
                            UpdateContractInfo(contract, salary);

                            // Update Account
                            if (empToSave.Account != null)
                            {
                                empToSave.Account.RoleID = SelectedRole?.RoleID ?? empToSave.Account.RoleID;
                                if (!string.IsNullOrEmpty(pBox.Password))
                                {
                                    empToSave.Account.Password = pBox.Password;
                                }
                            }
                        }
                        else
                        {
                            // INSERT (ADD NEW)
                            if (context.Accounts.Any(a => a.UserName == Username))
                            {
                                MessageBox.Show("Tên đăng nhập này đã tồn tại."); return;
                            }

                            string newId = GenerateNewEmployeeID(context);
                            empToSave = new Employee { EmployeeID = newId, Status = "Active" };
                            UpdateEmployeeInfo(empToSave);
                            context.Employees.Add(empToSave);

                            var newContract = new WorkContract { ContractID = "HD" + newId, EmployeeID = newId };
                            UpdateContractInfo(newContract, salary);
                            context.WorkContracts.Add(newContract);

                            if (SelectedRole == null) SelectedRole = Roles.FirstOrDefault();
                            context.Accounts.Add(new Account
                            {
                                UserID = "TK" + newId,
                                EmployeeID = newId,
                                UserName = Username,
                                Password = pBox.Password,
                                RoleID = SelectedRole?.RoleID,
                                IsActive = "Active"
                            });
                        }

                        context.SaveChanges();
                        transaction.Commit();

                        if (!string.IsNullOrEmpty(_selectedImagePath))
                        {
                            SaveImageToFolder(empToSave.EmployeeID, _selectedImagePath);
                        }

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
            }
        }

        [RelayCommand]
        private void Cancel(Window window) => window?.Close();

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
            var lastId = context.Employees.Where(e => e.EmployeeID.StartsWith("NV")).Select(e => e.EmployeeID).OrderByDescending(id => id).FirstOrDefault();
            if (string.IsNullOrEmpty(lastId)) return "NV001";
            if (int.TryParse(lastId.Substring(2), out int currentNum)) return "NV" + (currentNum + 1).ToString("D3");
            return "NV" + DateTime.Now.Ticks.ToString().Substring(0, 5);
        }

        private void SaveImageToFolder(string empId, string sourcePath)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string dest = Path.Combine(folder, $"{empId}{Path.GetExtension(sourcePath)}");
                File.Copy(sourcePath, dest, true);
            }
            catch { }
        }

        private string GetImagePath(string empId)
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
            string pathPng = Path.Combine(folder, $"{empId}.png");
            string pathJpg = Path.Combine(folder, $"{empId}.jpg");
            if (File.Exists(pathPng)) return pathPng;
            if (File.Exists(pathJpg)) return pathJpg;
            return "/Images/default_user.png";
        }
    }
}