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
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                string userImgFolder = Path.Combine(baseFolder, "Images", "EmployeeImages");
                if (!Directory.Exists(userImgFolder)) Directory.CreateDirectory(userImgFolder);
                string pathPng = Path.Combine(userImgFolder, $"{CurrentUser.Id}.png");
                string pathJpg = Path.Combine(userImgFolder, $"{CurrentUser.Id}.jpg");
                string pathDefault = Path.Combine(baseFolder, "Images", "default_user.png");
                string finalPathToLoad = "";

                if (File.Exists(pathPng))
                {
                    finalPathToLoad = pathPng; 
                }
                else if (File.Exists(pathJpg))
                {
                    finalPathToLoad = pathJpg; 
                }
                else
                {
                    if (File.Exists(pathDefault))
                    {
                        finalPathToLoad = pathDefault;
                    }
                    else
                    {
                        AvatarImage = null;
                        return;
                    }
                }
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; 
                bitmap.UriSource = new Uri(finalPathToLoad);
                bitmap.EndInit();
                bitmap.Freeze(); 
                AvatarImage = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi load ảnh: " + ex.Message);
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
                if (CurrentUser == null) return;
                int userIdToFind = CurrentUser.Id;
                using (var context = new DataContext())
                {
                    var empInDb = context.Employees.FirstOrDefault(e => e.Id == userIdToFind);
                    if (empInDb != null)
                    {
                        // Cho nay
                        empInDb.FirstName = CurrentUser.FirstName;
                        empInDb.LastName = CurrentUser.LastName;
                        empInDb.Email = CurrentUser.Email;
                        empInDb.PhoneNumber = CurrentUser.PhoneNumber;
                        empInDb.Address = CurrentUser.Address;
                        context.SaveChanges();
                        MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo");
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
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "EmployeeImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string ext = Path.GetExtension(sourcePath).ToLower(); 
                string destFile = Path.Combine(folder, $"{empId}{ext}");
                string oldPng = Path.Combine(folder, $"{empId}.png");
                string oldJpg = Path.Combine(folder, $"{empId}.jpg");
                if (File.Exists(oldPng)) File.Delete(oldPng);
                if (File.Exists(oldJpg)) File.Delete(oldJpg);
                File.Copy(sourcePath, destFile, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu ảnh: " + ex.Message);
            }
        }
    }
}