using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty] private Employee _currentUser;
    [ObservableProperty] private string _accountRole;
    [ObservableProperty] private string _username;
    [ObservableProperty] private bool _isEditing;

    public ProfileViewModel()
    {
        _ = LoadUserProfileAsync();
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

    private async Task LoadUserProfileAsync()
    {
        try
        {
            int currentId = UserSession.CurrentEmployeeId;
            if (currentId == 0) return;

            using var context = new DataContext();

            CurrentUser = await context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == currentId);

            var account = await context.Accounts
                .AsNoTracking()
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.EmployeeId == currentId);

            if (account != null)
            {
                Username = account.Username;
                AccountRole = account.Role?.RoleName ?? "N/A";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải thông tin cá nhân:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (string.IsNullOrWhiteSpace(CurrentUser.FirstName) || string.IsNullOrWhiteSpace(CurrentUser.LastName))
            {
                MessageBox.Show("Họ và Tên không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = new DataContext();
            var empInDb = await context.Employees.FindAsync(CurrentUser.Id);

            if (empInDb != null)
            {
                empInDb.FirstName = CurrentUser.FirstName;
                empInDb.LastName = CurrentUser.LastName;
                empInDb.Email = CurrentUser.Email;
                empInDb.PhoneNumber = CurrentUser.PhoneNumber;
                empInDb.Address = CurrentUser.Address;

                await context.SaveChangesAsync();

                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditing = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể lưu thay đổi:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ChangePassword()
    {
        MessageBox.Show("Vui lòng truy cập menu 'Đổi Mật Khẩu' trên thanh điều hướng để thực hiện chức năng này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}