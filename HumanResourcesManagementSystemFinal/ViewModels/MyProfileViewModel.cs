using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
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

        // Thêm biến tạm để lưu đường dẫn ảnh mới (trước khi bấm Lưu)
        private string _tempAvatarPath;

        public MyProfileViewModel()
        {
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                int currentId = UserSession.CurrentEmployeeId;

                using (var context = new DataContext())
                {
                    CurrentUser = context.Employees
                        .Include(e => e.Department)
                        .Include(e => e.Position)
                        .FirstOrDefault(e => e.Id == currentId);

                    var account = context.Accounts
                        .Include(a => a.Role)
                        .FirstOrDefault(a => a.EmployeeId == currentId);

                    AccountRole = account?.Role?.RoleName ?? "N/A";
                }

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
                string userImgFolder = Path.Combine(baseFolder, "Images", "EmployeeImages");

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(userImgFolder)) Directory.CreateDirectory(userImgFolder);

                string pathPng = Path.Combine(userImgFolder, $"{CurrentUser.Id}.png");
                string pathJpg = Path.Combine(userImgFolder, $"{CurrentUser.Id}.jpg");
                string pathDefault = Path.Combine(baseFolder, "Images", "default_user.png");

                string finalPath = "";

                if (File.Exists(pathPng)) finalPath = pathPng;
                else if (File.Exists(pathJpg)) finalPath = pathJpg;
                else if (File.Exists(pathDefault)) finalPath = pathDefault;
                else return; // Không có ảnh nào thì thôi

                // Load ảnh vào BitmapImage với CacheOption để không bị khóa file (giúp ghi đè được)
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
                System.Diagnostics.Debug.WriteLine("Lỗi load ảnh: " + ex.Message);
            }
        }

        [RelayCommand]
        private void ToggleEdit()
        {
            if (IsEditing)
            {
                // Nếu đang Edit mà bấm lần nữa -> Là hành động LƯU
                SaveChanges();
            }
            else
            {
                // Bắt đầu chỉnh sửa
                IsEditing = true;
            }
        }

        [RelayCommand]
        private void ChangeAvatar()
        {
            if (!IsEditing) return; // Chỉ cho đổi ảnh khi đang chế độ sửa

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Chưa lưu ngay vào DB, chỉ lưu tạm đường dẫn để hiển thị preview
                _tempAvatarPath = openFileDialog.FileName;

                // Hiển thị preview ngay lập tức
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_tempAvatarPath);
                bitmap.EndInit();
                AvatarImage = bitmap;
            }
        }

        private void SaveChanges()
        {
            try
            {
                if (CurrentUser == null) return;

                // Validate đơn giản
                if (string.IsNullOrWhiteSpace(CurrentUser.FirstName) || string.IsNullOrWhiteSpace(CurrentUser.LastName))
                {
                    MessageBox.Show("Họ và Tên không được để trống!", "Cảnh báo");
                    return;
                }

                using (var context = new DataContext())
                {
                    var empInDb = context.Employees.FirstOrDefault(e => e.Id == CurrentUser.Id);
                    if (empInDb != null)
                    {
                        // Cập nhật thông tin text
                        empInDb.FirstName = CurrentUser.FirstName;
                        empInDb.LastName = CurrentUser.LastName;
                        empInDb.Email = CurrentUser.Email;
                        empInDb.PhoneNumber = CurrentUser.PhoneNumber;
                        empInDb.Address = CurrentUser.Address;

                        context.SaveChanges();
                    }
                }

                // Nếu có thay đổi ảnh thì lưu file thật
                if (!string.IsNullOrEmpty(_tempAvatarPath))
                {
                    SaveImageToFolder(CurrentUser.Id, _tempAvatarPath);
                    _tempAvatarPath = null; // Reset biến tạm
                    LoadAvatarImage(); // Load lại từ file vừa lưu để đảm bảo chuẩn
                }

                MessageBox.Show("Cập nhật hồ sơ thành công!", "Thông báo");
                IsEditing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message);
            }
        }

        private void SaveImageToFolder(int empId, string sourcePath)
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string ext = Path.GetExtension(sourcePath).ToLower();
                string destFile = Path.Combine(folder, $"{empId}{ext}");

                // Xóa ảnh cũ (cả png và jpg để tránh trùng)
                string oldPng = Path.Combine(folder, $"{empId}.png");
                string oldJpg = Path.Combine(folder, $"{empId}.jpg");

                if (File.Exists(oldPng)) File.Delete(oldPng);
                if (File.Exists(oldJpg)) File.Delete(oldJpg);

                File.Copy(sourcePath, destFile, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu ảnh: " + ex.Message);
            }
        }
    }
}