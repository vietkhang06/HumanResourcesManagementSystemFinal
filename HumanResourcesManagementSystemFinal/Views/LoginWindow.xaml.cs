using System.Windows;
using HumanResourcesManagementSystemFinal.ViewModels;

namespace HumanResourcesManagementSystemFinal.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        ShowLogin();
    }

    public void ShowLogin()
    {
        var loginVM = new LoginViewModel();
        loginVM.NavigateToForgotPasswordAction = () =>
        {
            ShowForgotPassword();
        };

        var loginView = new LoginControl();
        loginView.DataContext = loginVM;
        MainFrame.Content = loginView;
    }

    public void ShowForgotPassword()
    {
        var forgotVM = new ForgotPasswordViewModel();
        forgotVM.NavigateToLoginAction = () => ShowLogin();
        var forgotView = new ForgotPasswordControl();
        forgotView.DataContext = forgotVM;
        MainFrame.Content = forgotView;
    }
}