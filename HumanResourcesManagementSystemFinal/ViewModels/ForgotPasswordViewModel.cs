using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    public Action? NavigateToLoginAction { get; set; }
    private readonly EmailService _emailService;

    public ForgotPasswordViewModel()
    {
        _emailService = new EmailService();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitRequestCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitRequestCommand))]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitRequestCommand))]
    private string _emailAddress = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitRequestCommand))]
    private string _cccd = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitRequestCommand))]
    private bool _isBusy = false;

    [RelayCommand]
    private void SwitchToLogin()
    {
        NavigateToLoginAction?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanSubmitRequest))]
    private async Task SubmitRequestAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(EmailAddress) ||
            string.IsNullOrWhiteSpace(PhoneNumber) )
        {
            MessageBox.Show(
                "Vui lòng nhập đầy đủ thông tin: Tên đăng nhập, Email, SĐT",
                "Thiếu thông tin",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (!IsValidEmail(EmailAddress))
        {
            MessageBox.Show(
                "Định dạng Email không hợp lệ.",
                "Sai định dạng",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            string existingPassword = null;
            string targetEmail = string.Empty;

            using (var context = new DataContext())
            {
                var account = await context.Accounts
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(a => a.UserName == Username);

                if (account != null && account.Employee != null)
                {
                    string dbEmail = account.Employee.Email ?? string.Empty;
                    string dbPhone = account.Employee.PhoneNumber ?? string.Empty;

                    bool isEmailMatch = dbEmail.Trim()
                        .Equals(EmailAddress.Trim(), StringComparison.OrdinalIgnoreCase);
                    bool isPhoneMatch = dbPhone.Trim() == PhoneNumber.Trim();

                    if (isEmailMatch && isPhoneMatch)
                    {
                        existingPassword = account.Password;
                        targetEmail = dbEmail;
                    }
                }
            }

            if (!string.IsNullOrEmpty(existingPassword))
            {
                await _emailService.SendPassResetEmailAsync(targetEmail, existingPassword);

                MessageBox.Show(
                    $"Mật khẩu đã được gửi tới email:\n{targetEmail}\n\nVui lòng kiểm tra hộp thư (bao gồm cả mục Spam).",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                SwitchToLogin();
            }
            else
            {
                MessageBox.Show(
                    "Thông tin xác thực không chính xác.\nVui lòng kiểm tra lại Tên đăng nhập, Email, SĐT.",
                    "Không tìm thấy tài khoản",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (System.Net.Mail.SmtpException smtpEx)
        {
            MessageBox.Show(
                $"Lỗi gửi Email: {smtpEx.Message}\nVui lòng kiểm tra kết nối mạng.",
                "Lỗi Gửi Mail",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Lỗi hệ thống: {ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            Mouse.OverrideCursor = null;
        }
    }

    private bool CanSubmitRequest()
    {
        return !_isBusy &&
               !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(EmailAddress) &&
               !string.IsNullOrWhiteSpace(PhoneNumber);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
