using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ChangePasswordViewModel : ObservableObject
    {
        private readonly AccountService _accountService;
        private readonly string _currentUserId;

        public ChangePasswordViewModel(string userId)
        {
            _currentUserId = userId;
            _accountService = new AccountService(new DataContext());
        }

        [RelayCommand]
        public async Task ChangePassword(object parameter)
        {
            if (parameter is not object[] values || values.Length < 3) return;

            var oldPassBox = values[0] as PasswordBox;
            var newPassBox = values[1] as PasswordBox;
            var confirmPassBox = values[2] as PasswordBox;

            if (oldPassBox == null || newPassBox == null || confirmPassBox == null) return;

            string oldPass = oldPassBox.Password;
            string newPass = newPassBox.Password;
            string confirmPass = confirmPassBox.Password;

            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass.Length < 3)
            {
                MessageBox.Show("Mật khẩu mới quá ngắn (tối thiểu 3 ký tự)!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success = await _accountService.ChangePasswordAsync(_currentUserId, oldPass, newPass);
                if (success)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    oldPassBox.Clear();
                    newPassBox.Clear();
                    confirmPassBox.Clear();
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex.Message);

                var inner = ex.InnerException;
                while (inner != null)
                {
                    sb.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }

                MessageBox.Show("Lỗi khi đổi mật khẩu:\n" + sb.ToString(), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}