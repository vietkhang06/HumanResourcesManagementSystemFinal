using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class MyProfileViewModel : ObservableObject
    {
        [ObservableProperty] private Employee _currentUser;
        [ObservableProperty] private string _accountRole;
        [ObservableProperty] private bool _isEditing;

        public MyProfileViewModel()
        {
            IsEditing = false;
        }

        public MyProfileViewModel(string employeeId)
        {
            IsEditing = false;
            if (!string.IsNullOrEmpty(employeeId))
            {
                _ = LoadUserProfile(employeeId);
            }
        }

        public async Task LoadUserProfile(string inputId)
        {
            try
            {
                using var context = new DataContext();
                var emp = await context.Employees
                    .AsNoTracking()
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.Account).ThenInclude(a => a.Role)
                    .FirstOrDefaultAsync(e => e.EmployeeID == inputId);

                if (emp != null)
                {
                    CurrentUser = emp;
                    if (emp.Account != null && emp.Account.Role != null)
                        AccountRole = emp.Account.Role.RoleName;
                    else
                        AccountRole = "Nhân viên";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hồ sơ: " + ex.Message);
            }
        }

        [RelayCommand]
        private async Task ToggleEdit()
        {
            if (IsEditing)
            {
                bool success = await SaveProfileToDatabase();
                if (success)
                {
                    IsEditing = false;
                    MessageBox.Show("Cập nhật hồ sơ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                IsEditing = true;
            }
        }

        private async Task<bool> SaveProfileToDatabase()
        {
            try
            {
                using var context = new DataContext();
                var empInDb = await context.Employees.FindAsync(CurrentUser.EmployeeID);

                if (empInDb != null)
                {
                    empInDb.FullName = CurrentUser.FullName;
                    empInDb.Email = CurrentUser.Email;
                    empInDb.PhoneNumber = CurrentUser.PhoneNumber;
                    empInDb.Address = CurrentUser.Address;

                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        [RelayCommand]
        private void ChangeAvatar()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Title = "Chọn ảnh đại diện"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string sourceFilePath = openFileDialog.FileName;
                    string extension = Path.GetExtension(sourceFilePath);
                    string projectPath = AppDomain.CurrentDomain.BaseDirectory;
                    string imageFolder = Path.Combine(projectPath, "Images", "Profiles");

                    if (!Directory.Exists(imageFolder))
                    {
                        Directory.CreateDirectory(imageFolder);
                    }

                    string[] oldFiles = Directory.GetFiles(imageFolder, $"{CurrentUser.EmployeeID}.*");
                    foreach (var file in oldFiles)
                    {
                        try { File.Delete(file); } catch { }
                    }

                    string destFileName = $"{CurrentUser.EmployeeID}{extension}";
                    string destFilePath = Path.Combine(imageFolder, destFileName);

                    File.Copy(sourceFilePath, destFilePath, true);

                    string tempId = CurrentUser.EmployeeID;
                    CurrentUser.EmployeeID = "TEMP_REFRESH";
                    OnPropertyChanged(nameof(CurrentUser));
                    CurrentUser.EmployeeID = tempId;
                    OnPropertyChanged(nameof(CurrentUser));

                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<string>(CurrentUser.EmployeeID));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đổi ảnh: {ex.Message}\nCó thể file đang được mở bởi ứng dụng khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}