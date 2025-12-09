using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; 

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ChangePasswordViewModel : ObservableObject
    {
        private readonly AccountService _accountService;
        private readonly int _currentAccountId;

        public ChangePasswordViewModel(int accountId)
        {
            _currentAccountId = accountId;
            _accountService = new AccountService(new DataContext());
        }

        [RelayCommand]
        public async Task ChangePassword(object parameter)
        {
            var values = (object[])parameter;
            var oldPassBox = (PasswordBox)values[0];
            var newPassBox = (PasswordBox)values[1];
            var confirmPassBox = (PasswordBox)values[2];
            string oldPass = oldPassBox.Password;
            string newPass = newPassBox.Password;
            string confirmPass = confirmPassBox.Password;
            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass.Length < 3) 
            {
                MessageBox.Show("Mật khẩu mới quá ngắn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                bool success = await _accountService.ChangePasswordAsync(_currentAccountId, oldPass, newPass);
                if (success)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo");
                    oldPassBox.Password = "";
                    newPassBox.Password = "";
                    confirmPassBox.Password = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}