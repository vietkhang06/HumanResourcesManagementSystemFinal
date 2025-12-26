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
using System.Text;
using System.Threading.Tasks;
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

        // Giữ lại FirstName/LastName để Binding với View, sau đó sẽ gộp lại khi Lưu
        [ObservableProperty] private string _firstName;
        [ObservableProperty] private string _lastName;

        [ObservableProperty] private string _cccd; // Thêm CCCD
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

        public AddEmployeeViewModel()
        {
            LoadComboBoxData();
        }

        // --- HÀM SINH MÃ TỰ ĐỘNG NV001 ---
        private string GenerateNewEmployeeID(DataContext context)
        {
            var lastEmp = context.Employees
                .OrderByDescending(e => e.EmployeeID)
                .FirstOrDefault();

            if (lastEmp == null || string.IsNullOrEmpty(lastEmp.EmployeeID)) return "NV001";

            // Giả sử mã dạng NVxxx
            if (lastEmp.EmployeeID.Length > 2)
            {
                string numPart = lastEmp.EmployeeID.Substring(2);
                if (int.TryParse(numPart, out int num))
                {
                    return "NV" + (num + 1).ToString("D3");
                }
            }
            return "NV" + (new Random().Next(100, 999));
        }
        // ----------------------------------

        public void LoadEmployeeForEdit(Employee emp)
        {
            try
            {
                _editingEmployee = emp;
                WindowTitle = "CẬP NHẬT THÔNG TIN";
                OnPropertyChanged(nameof(IsEditMode));

                // 1. Tách FullName ra thành First/Last để hiển thị lên View
                if (!string.IsNullOrEmpty(emp.FullName))
                {
                    var parts = emp.FullName.Trim().Split(' ');
                    if (parts.Length > 0)
                    {
                        LastName = parts[parts.Length - 1]; // Lấy từ cuối làm Tên
                        if (parts.Length > 1)
                            FirstName = string.Join(" ", parts.Take(parts.Length - 1)); // Các từ trước là Họ Đệm
                        else
                            FirstName = "";
                    }
                }

                Email = emp.Email;
                Phone = emp.PhoneNumber;
                Address = emp.Address;
                _cccd = emp.CCCD; // Load CCCD
                BirthDate = emp.DateOfBirth ?? DateTime.Now;
                Gender = emp.Gender;

                // Load ảnh theo String ID
                string imagePath = GetImagePath(emp.EmployeeID);
                if (!string.IsNullOrEmpty(imagePath)) AvatarSource = imagePath;

                SelectedDepartment = Departments.FirstOrDefault(d => d.DepartmentID == emp.DepartmentID);
                SelectedPosition = Positions.FirstOrDefault(p => p.PositionID == emp.PositionID);
                SelectedManager = Managers.FirstOrDefault(m => m.EmployeeID == emp.ManagerID);

                using (var context = new DataContext())
                {
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
                            Username = fullEmp.Account.UserName; // Model mới UserName
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

                    var defaultRole = Roles.FirstOrDefault(r => r.RoleName.Contains("Employee") || r.RoleName.Contains("Nhân viên"));
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

            // Validation
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Vui lòng nhập Họ và Tên.", "Cảnh báo"); return;
            }
            if (SelectedDepartment == null || SelectedPosition == null)
            {
                MessageBox.Show("Vui lòng chọn Phòng ban/Chức vụ.", "Cảnh báo"); return;
            }
            if (!decimal.TryParse(SalaryString, out decimal salary))
            {
                MessageBox.Show("Lương phải là số.", "Cảnh báo"); return;
            }

            // Gộp họ tên thành FullName
            string fullName = $"{FirstName} {LastName}".Trim();

            using (var context = new DataContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        Employee empToSave;

                        // --- TRƯỜNG HỢP UPDATE ---
                        if (IsEditMode)
                        {
                            empToSave = context.Employees
                                .Include(e => e.Account).Include(e => e.WorkContracts)
                                .FirstOrDefault(e => e.EmployeeID == _editingEmployee.EmployeeID);

                            if (empToSave == null) return;

                            empToSave.FullName = fullName;
                            empToSave.CCCD = _cccd;
                            empToSave.Email = Email;
                            empToSave.PhoneNumber = Phone;
                            empToSave.Address = Address;
                            empToSave.DateOfBirth = BirthDate;
                            empToSave.Gender = Gender;
                            empToSave.DepartmentID = SelectedDepartment.DepartmentID;
                            empToSave.PositionID = SelectedPosition.PositionID;
                            empToSave.ManagerID = SelectedManager?.EmployeeID;

                            // Update Hợp đồng mới nhất
                            var contract = empToSave.WorkContracts.OrderByDescending(c => c.StartDate).FirstOrDefault();
                            if (contract == null)
                            {
                                // Tạo mới nếu chưa có
                                contract = new WorkContract
                                {
                                    ContractID = "HD" + empToSave.EmployeeID.Substring(2),
                                    EmployeeID = empToSave.EmployeeID
                                };
                                context.WorkContracts.Add(contract);
                            }
                            contract.Salary = salary;
                            contract.ContractType = ContractType;
                            contract.StartDate = StartDate;
                            contract.EndDate = EndDate;

                            // Update Tài khoản
                            if (empToSave.Account != null)
                            {
                                empToSave.Account.RoleID = SelectedRole?.RoleID ?? empToSave.Account.RoleID;
                                if (!string.IsNullOrEmpty(pBox.Password))
                                    empToSave.Account.Password = pBox.Password; // Lưu Password mới
                            }
                        }
                        // --- TRƯỜNG HỢP CREATE ---
                        else
                        {
                            if (context.Accounts.Any(a => a.UserName == Username))
                            {
                                MessageBox.Show("Username đã tồn tại."); return;
                            }

                            string newId = GenerateNewEmployeeID(context); // Sinh mã NVxxx

                            empToSave = new Employee
                            {
                                EmployeeID = newId,
                                FullName = fullName,
                                CCCD = _cccd,
                                Email = Email,
                                PhoneNumber = Phone,
                                Address = Address,
                                DateOfBirth = BirthDate,
                                Gender = Gender,
                                DepartmentID = SelectedDepartment.DepartmentID,
                                PositionID = SelectedPosition.PositionID,
                                ManagerID = SelectedManager?.EmployeeID,
                                Status = "Active"
                            };
                            context.Employees.Add(empToSave);

                            // Tạo hợp đồng (Mã HD đồng bộ với mã NV)
                            context.WorkContracts.Add(new WorkContract
                            {
                                ContractID = "HD" + newId.Substring(2),
                                EmployeeID = newId,
                                ContractType = ContractType,
                                Salary = salary,
                                StartDate = StartDate,
                                EndDate = EndDate
                            });

                            // Tạo tài khoản (Mã TK đồng bộ với mã NV)
                            if (SelectedRole == null) SelectedRole = Roles.FirstOrDefault();
                            context.Accounts.Add(new Account
                            {
                                UserID = "TK" + newId.Substring(2),
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
                            SaveImageToFolder(empToSave.EmployeeID, _selectedImagePath);

                        MessageBox.Show("Lưu thành công!");
                        window.DialogResult = true;
                        window.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi: " + ex.Message);
                    }
                }
            }
        }

        [RelayCommand]
        private void Cancel(Window window) => window?.Close();

        private void SaveImageToFolder(string empId, string sourcePath)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                // Lưu ảnh với tên là ID nhân viên
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
            return "";
        }
    }
}