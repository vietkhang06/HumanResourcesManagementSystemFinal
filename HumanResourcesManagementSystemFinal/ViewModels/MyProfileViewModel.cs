using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class MyProfileViewModel : ObservableObject
    {
        [ObservableProperty]
        private Employee _currentUser;

        [ObservableProperty]
        private string _accountRole;

        [ObservableProperty]
        private BitmapImage _avatarImage;

        [ObservableProperty]
        private bool _isEditing;

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
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadAvatarImage()
        {
            if (CurrentUser == null) return;

            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                string path = Path.Combine(folder, $"{CurrentUser.Id}.png");

                if (!File.Exists(path))
                {
                    path = Path.Combine(folder, $"{CurrentUser.Id}.jpg");
                }

                if (File.Exists(path))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    AvatarImage = bitmap;
                }
                else
                {
                    AvatarImage = new BitmapImage(new Uri("pack://application:,,,/Images/default_avatar.png"));
                }
            }
            catch
            {
                AvatarImage = null;
            }
        }

        [RelayCommand]
        private void ToggleEdit()
        {
            if (IsEditing)
            {
                SaveChanges();
            }
            IsEditing = !IsEditing;
        }

        [RelayCommand]
        private void ChangeAvatar()
        {
            if (!IsEditing) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg)|*.png;*.jpg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SaveImageToFolder(CurrentUser.Id, openFileDialog.FileName);
                LoadAvatarImage();
            }
        }

        private void SaveChanges()
        {
            try
            {
                // ✅ BƯỚC 1: Kiểm tra null và lấy ID ra biến cục bộ (QUAN TRỌNG NHẤT)
                // Việc này giúp EF Core hiểu đây là một con số int đơn giản, tránh lỗi LINQ
                if (CurrentUser == null) return;
                int userIdToFind = CurrentUser.Id;

                using (var context = new DataContext())
                {
                    // ✅ BƯỚC 2: Dùng biến 'userIdToFind' thay vì 'CurrentUser.Id'
                    var empInDb = context.Employees.FirstOrDefault(e => e.Id == userIdToFind);

                    if (empInDb != null)
                    {
                        // Cập nhật dữ liệu từ giao diện vào Database
                        empInDb.FirstName = CurrentUser.FirstName;
                        empInDb.LastName = CurrentUser.LastName;
                        empInDb.Email = CurrentUser.Email;
                        empInDb.PhoneNumber = CurrentUser.PhoneNumber;
                        empInDb.Address = CurrentUser.Address;

                        // Lưu xuống DB
                        context.SaveChanges();

                        MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo");

                        // Tự động tắt chế độ chỉnh sửa sau khi lưu xong
                        IsEditing = false;
                    }
                }
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
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string ext = Path.GetExtension(sourcePath);
                string destFile = Path.Combine(folder, $"{empId}{ext}");

                File.Copy(sourcePath, destFile, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu ảnh: " + ex.Message);
            }
        }
    }
}