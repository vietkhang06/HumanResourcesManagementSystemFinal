using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Services;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ChangePasswordViewModel : ObservableObject
    {
        private readonly AccountService accountService;
        private readonly string currentUserId;

        public ChangePasswordViewModel(string userId)
        {
            currentUserId = userId;
            accountService = new AccountService(new DataContext());
        }

        [RelayCommand]
        public async Task ChangePassword(object parameter)
        {
            if (parameter is not object[] values || values.Length < 3) return;

            if (values[0] is not PasswordBox oldPassBox ||
                values[1] is not PasswordBox newPassBox ||
                values[2] is not PasswordBox confirmPassBox) return;

            var oldPass = oldPassBox.Password;
            var newPass = newPassBox.Password;
            var confirmPass = confirmPassBox.Password;

            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            if (newPass.Length < 3)
            {
                MessageBox.Show("Mật khẩu mới quá ngắn!");
                return;
            }

            try
            {
                if (await accountService.ChangePasswordAsync(currentUserId, oldPass, newPass))
                {
                    MessageBox.Show("Đổi mật khẩu thành công!");
                    oldPassBox.Clear();
                    newPassBox.Clear();
                    confirmPassBox.Clear();
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder(ex.Message);
                var inner = ex.InnerException;

                while (inner != null)
                {
                    sb.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }

                MessageBox.Show(sb.ToString(), "Lỗi");
            }
        }
    }
}
