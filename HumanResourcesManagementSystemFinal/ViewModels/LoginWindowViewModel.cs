using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class LoginWindowViewModel : ObservableObject
{
    public LoginViewModel LoginVM { get; }
    public ForgotPasswordViewModel ForgotPasswordVM { get; }
    private object _currentView;

    public object CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public LoginWindowViewModel()
    {
        LoginVM = new LoginViewModel();
        ForgotPasswordVM = new ForgotPasswordViewModel();

        LoginVM.NavigateToForgotPasswordAction = () => CurrentView = ForgotPasswordVM;
        ForgotPasswordVM.NavigateToLoginAction = () => CurrentView = LoginVM;

        _currentView = LoginVM;
        OnPropertyChanged(nameof(CurrentView));
    }
}