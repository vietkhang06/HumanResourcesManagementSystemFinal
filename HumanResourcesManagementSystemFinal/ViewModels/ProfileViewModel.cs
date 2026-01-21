using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        [ObservableProperty] private Employee _currentUser;
        [ObservableProperty] private string _accountRole;
        [ObservableProperty] private string _username;
        [ObservableProperty] private bool _isEditing;

        public ProfileViewModel()
        {
        }

        private string GetDeepErrorMessage(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);
            var inner = ex.InnerException;
            while (inner != null)
            {
                sb.AppendLine(inner.Message);
                inner = inner.InnerException;
            }
            return sb.ToString();
        }

        public async Task LoadUserProfileAsync()
        {
            try
            {
                string currentId = UserSession.CurrentEmployeeId;
                if (string.IsNullOrEmpty(currentId)) return;

                using var context = new DataContext();

                var emp = await context.Employees
                    .AsNoTracking()
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.Account)
                    .ThenInclude(a => a.Role)
                    .FirstOrDefaultAsync(e => e.EmployeeID == currentId);

                if (emp != null)
                {
                    CurrentUser = emp;

                    if (emp.Account != null)
                    {
                        Username = emp.Account.UserName;
                        AccountRole = emp.Account.Role?.RoleName ?? "N/A";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin:\n" + GetDeepErrorMessage(ex));
            }
        }

        [RelayCommand]
        private async Task ToggleEditAsync()
        {
            if (IsEditing)
            {
                await SaveChangesAsync();
            }
            else
            {
                IsEditing = true;
            }
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                if (CurrentUser == null) return;

                if (string.IsNullOrWhiteSpace(CurrentUser.FullName))
                {
                    MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var context = new DataContext();

                context.Employees.Attach(CurrentUser);
                context.Entry(CurrentUser).State = EntityState.Modified;

                if (CurrentUser.Account != null)
                {
                    context.Accounts.Attach(CurrentUser.Account);
                    context.Entry(CurrentUser.Account).State = EntityState.Modified;
                }

                await context.SaveChangesAsync();

                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                IsEditing = false;
                await LoadUserProfileAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu thay đổi:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ChangeAvatar()
        {
            try
            {
                if (CurrentUser.Account == null)
                {
                    MessageBox.Show("Lỗi: Không tìm thấy tài khoản liên kết!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Title = "Chọn ảnh đại diện"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    byte[] fileBytes = File.ReadAllBytes(openFileDialog.FileName);
                    CurrentUser.Account.AvatarData = fileBytes;
                    OnPropertyChanged(nameof(CurrentUser));

                    MessageBox.Show("Đã chọn ảnh mới. Hãy nhấn 'Lưu Thay Đổi' để lưu vào CSDL.", "Thông báo");
                    IsEditing = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đọc ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ChangePassword()
        {
            MessageBox.Show("Vui lòng truy cập menu 'Đổi Mật Khẩu'.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}