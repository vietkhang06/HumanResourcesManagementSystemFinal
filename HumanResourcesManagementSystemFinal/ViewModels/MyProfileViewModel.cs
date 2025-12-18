using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using HumanResourcesManagementSystemFinal.Services; // Cần namespace này để dùng UserSession
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class MyProfileViewModel : ObservableObject
    {
        [ObservableProperty] private Employee _currentUser;
        [ObservableProperty] private string _accountRole;
        [ObservableProperty] private BitmapImage _avatarImage;
        [ObservableProperty] private bool _isEditing;

        // Biến tạm để lưu đường dẫn ảnh khi chọn (Preview)
        private string _tempAvatarPath;

        // Tên thư mục chứa ảnh (Đồng bộ với AddEmployeeViewModel)
        private const string ImageFolderName = "EmployeeImages";

        public MyProfileViewModel()
        {
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                // 1. Lấy ID từ Session đăng nhập
                int currentId = UserSession.CurrentEmployeeId;

                if (currentId == 0)
                {
                    // Fallback nếu chạy Design Mode hoặc lỗi session
                    return;
                }

                using (var context = new DataContext())
                {
                    // 2. Load thông tin nhân viên + Phòng ban + Chức vụ
                    CurrentUser = context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.Position)
                        .FirstOrDefault(e => e.Id == currentId);

                    // 3. Load Role từ bảng Account
                    var account = context.Accounts
                        .Include(a => a.Role)
                        .FirstOrDefault(a => a.EmployeeId == currentId);

                    AccountRole = account?.Role?.RoleName ?? "N/A";
                }

                // 4. Load Ảnh đại diện
                LoadAvatarImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hồ sơ: " + ex.Message);
            }
        }

        private void LoadAvatarImage()
        {
            if (CurrentUser == null) return;

            try
            {
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = Path.Combine(baseFolder, ImageFolderName);

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Ưu tiên tìm .png, sau đó .jpg
                string pathPng = Path.Combine(folderPath, $"{CurrentUser.Id}.png");
                string pathJpg = Path.Combine(folderPath, $"{CurrentUser.Id}.jpg");

                // Ảnh mặc định (nằm trong thư mục Images của project)
                string pathDefault = Path.Combine(baseFolder, "Images", "default_user.png");

                string finalPath = "";

                if (File.Exists(pathPng)) finalPath = pathPng;
                else if (File.Exists(pathJpg)) finalPath = pathJpg;
                else if (File.Exists(pathDefault)) finalPath = pathDefault;
                else return; // Không có ảnh nào cả

                // Kỹ thuật load ảnh không khóa file (BitmapCacheOption.OnLoad)
                // Giúp có thể ghi đè/xóa file ảnh ngay cả khi đang hiển thị
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(finalPath);
                bitmap.EndInit();
                bitmap.Freeze(); // Tối ưu hiệu năng cho WPF

                AvatarImage = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi load ảnh: " + ex.Message);
            }
        }

        [RelayCommand]
        private void ToggleEdit()
        {
            if (IsEditing)
            {
                // Nếu đang ở chế độ Sửa mà bấm nút -> Thực hiện LƯU
                SaveChanges();
            }
            else
            {
                // Chuyển sang chế độ Sửa
                IsEditing = true;
            }
        }

        [RelayCommand]
        private void ChangeAvatar()
        {
            if (!IsEditing) return; // Chỉ cho phép đổi ảnh khi đang nhấn "Chỉnh sửa"

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Chọn ảnh đại diện"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Lưu tạm đường dẫn vào biến
                _tempAvatarPath = openFileDialog.FileName;

                // Hiển thị Preview ngay lập tức lên giao diện
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_tempAvatarPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                AvatarImage = bitmap;
            }
        }

        private void SaveChanges()
        {
            try
            {
                if (CurrentUser == null) return;

                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(CurrentUser.FirstName) || string.IsNullOrWhiteSpace(CurrentUser.LastName))
                {
                    MessageBox.Show("Họ và Tên không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 1. Lưu thông tin Text vào Database
                using (var context = new DataContext())
                {
                    var empInDb = context.Employees.FirstOrDefault(e => e.Id == CurrentUser.Id);
                    if (empInDb != null)
                    {
                        empInDb.FirstName = CurrentUser.FirstName;
                        empInDb.LastName = CurrentUser.LastName;
                        empInDb.Email = CurrentUser.Email;
                        empInDb.PhoneNumber = CurrentUser.PhoneNumber;
                        empInDb.Address = CurrentUser.Address;

                        context.SaveChanges();
                    }
                }

                // 2. Lưu file ảnh thật (nếu có thay đổi)
                if (!string.IsNullOrEmpty(_tempAvatarPath))
                {
                    SaveImageToFolder(CurrentUser.Id, _tempAvatarPath);
                    _tempAvatarPath = null; // Reset biến tạm

                    // Load lại ảnh từ file vừa lưu để đảm bảo đồng bộ
                    LoadAvatarImage();
                }

                MessageBox.Show("Cập nhật hồ sơ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveImageToFolder(int empId, string sourcePath)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImageFolderName);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string ext = Path.GetExtension(sourcePath).ToLower();
                string destFile = Path.Combine(folder, $"{empId}{ext}");

                // Xóa ảnh cũ (cả png và jpg) để tránh tồn tại song song gây nhầm lẫn
                string oldPng = Path.Combine(folder, $"{empId}.png");
                string oldJpg = Path.Combine(folder, $"{empId}.jpg");

                if (File.Exists(oldPng)) File.Delete(oldPng);
                if (File.Exists(oldJpg)) File.Delete(oldJpg);

                // Copy file mới vào
                File.Copy(sourcePath, destFile, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu ảnh vào thư mục hệ thống: " + ex.Message);
            }
        }
    }
}