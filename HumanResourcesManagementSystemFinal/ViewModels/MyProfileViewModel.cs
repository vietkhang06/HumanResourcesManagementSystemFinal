using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class MyProfileViewModel : ObservableObject
{
    [ObservableProperty] private Employee _currentUser;
    [ObservableProperty] private string _accountRole;
    [ObservableProperty] private BitmapImage _avatarImage;
    [ObservableProperty] private bool _isEditing;

    private string _tempAvatarPath;
    private const string ImageFolderName = "EmployeeImages";

    public MyProfileViewModel()
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
            // 1. Đổi int -> string (Lấy từ Session mới)
            string currentId = UserSession.CurrentEmployeeId.ToString();
            if (string.IsNullOrEmpty(currentId)) return;

            using var context = new DataContext();

            // 2. Tìm theo EmployeeID (string)
            CurrentUser = await context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.EmployeeID == currentId);

            var account = await context.Accounts
                .AsNoTracking()
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.EmployeeID == currentId);

            AccountRole = account?.Role?.RoleName ?? "N/A";

            LoadAvatarImage();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải hồ sơ:\n" + GetDeepErrorMessage(ex), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadAvatarImage()
    {
        if (CurrentUser == null) return;

        try
        {
            string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseFolder, ImageFolderName);

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            // 3. Dùng EmployeeID làm tên file
            string pathPng = Path.Combine(folderPath, $"{CurrentUser.EmployeeID}.png");
            string pathJpg = Path.Combine(folderPath, $"{CurrentUser.EmployeeID}.jpg");
            string pathDefault = Path.Combine(baseFolder, "Images", "default_user.png");

            string finalPath = string.Empty;

            if (File.Exists(pathPng)) finalPath = pathPng;
            else if (File.Exists(pathJpg)) finalPath = pathJpg;
            else if (File.Exists(pathDefault)) finalPath = pathDefault;
            else return;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(finalPath);
            bitmap.EndInit();
            bitmap.Freeze();

            AvatarImage = bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Lỗi hiển thị ảnh: " + ex.Message);
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

    [RelayCommand]
    private void ChangeAvatar()
    {
        if (!IsEditing) return;

        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            Title = "Chọn ảnh đại diện"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _tempAvatarPath = openFileDialog.FileName;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_tempAvatarPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                AvatarImage = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc file ảnh này: " + ex.Message);
            }
        }
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            if (CurrentUser == null) return;

            // 4. Validate FullName
            if (string.IsNullOrWhiteSpace(CurrentUser.FullName))
            {
                MessageBox.Show("Họ và Tên không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = new DataContext();
            var empInDb = await context.Employees.FindAsync(CurrentUser.EmployeeID);

            if (empInDb != null)
            {
                // 5. Cập nhật FullName
                empInDb.FullName = CurrentUser.FullName;

                // Nếu view binding trực tiếp vào CurrentUser thì các trường khác đã tự cập nhật
                empInDb.Email = CurrentUser.Email;
                empInDb.PhoneNumber = CurrentUser.PhoneNumber;
                empInDb.Address = CurrentUser.Address;
                // empInDb.CCCD = CurrentUser.CCCD; // Nếu muốn cho phép sửa CCCD thì thêm dòng này

                await context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(_tempAvatarPath))
            {
                SaveImageToFolder(CurrentUser.EmployeeID, _tempAvatarPath);
                _tempAvatarPath = null;
                LoadAvatarImage();
            }

            MessageBox.Show("Cập nhật hồ sơ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            IsEditing = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi lưu dữ liệu:\n" + GetDeepErrorMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 6. Sửa tham số int -> string
    private void SaveImageToFolder(string empId, string sourcePath)
    {
        try
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImageFolderName);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string ext = Path.GetExtension(sourcePath).ToLower();
            string destFile = Path.Combine(folder, $"{empId}{ext}");

            string oldPng = Path.Combine(folder, $"{empId}.png");
            string oldJpg = Path.Combine(folder, $"{empId}.jpg");

            // Xóa ảnh cũ để tránh trùng lặp định dạng
            if (File.Exists(oldPng)) File.Delete(oldPng);
            if (File.Exists(oldJpg)) File.Delete(oldJpg);

            File.Copy(sourcePath, destFile, true);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể lưu ảnh vào hệ thống:\n" + GetDeepErrorMessage(ex), "Lỗi IO", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}