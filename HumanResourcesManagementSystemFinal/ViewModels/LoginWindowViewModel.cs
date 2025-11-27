using CommunityToolkit.Mvvm.ComponentModel;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class LoginWindowViewModel : ObservableObject
{
    // Khai báo 2 màn hình con
    public LoginViewModel LoginVM { get; }
    public ForgotPasswordViewModel ForgotPasswordVM { get; }

    // Biến lưu màn hình hiện tại
    private object _currentView;
    public object CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public LoginWindowViewModel()
    {
        // 1. Khởi tạo 2 màn hình con
        LoginVM = new LoginViewModel();
        ForgotPasswordVM = new ForgotPasswordViewModel();

        // 2. Thiết lập hành động chuyển trang (Action)
        LoginVM.NavigateToForgotPasswordAction = () => CurrentView = ForgotPasswordVM;
        ForgotPasswordVM.NavigateToLoginAction = () => CurrentView = LoginVM;

        // 3. Gán giá trị mặc định ban đầu (Quan trọng để không bị màn hình trắng)
        _currentView = LoginVM;
        OnPropertyChanged(nameof(CurrentView));
    }
}