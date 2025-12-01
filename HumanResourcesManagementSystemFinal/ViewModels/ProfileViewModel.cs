using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesManagementSystemFinal.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty] private Employee _currentUser;
    [ObservableProperty] private string _accountRole;
    [ObservableProperty] private string _username;
    [ObservableProperty] private bool _isEditing;

    public ProfileViewModel()
    {
        LoadUserProfile();
    }

    private void LoadUserProfile()
    {
        CurrentUser = new Employee
        {
            Id = 1,
            FirstName = "Admin",
            LastName = "Nguyễn Văn",
            Email = "admin@hrms.com",
            PhoneNumber = "0901234567",
            Department = new Department { DepartmentName = "Ban Giám Đốc" },
            Position = new Position { Title = "System Admin" } // Đã sửa PositionName -> Title
        };

        Username = "admin";
        AccountRole = "Administrator";
        IsEditing = false;
    }

    [RelayCommand]
    private void ToggleEdit()
    {
        IsEditing = !IsEditing;
    }

    [RelayCommand]
    private void ChangePassword()
    {
    }
}