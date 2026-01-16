using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class AddPositionViewModel : ObservableObject
    {
        [ObservableProperty] private string _title = "Thêm Chức Vụ Mới";
        [ObservableProperty] private string _posTitle;
        [ObservableProperty] private string _jobDescription;

        public AddPositionViewModel(){}

        public AddPositionViewModel(Position existingPos)
        {
            Title = "Cập Nhật Chức Vụ";
            PosTitle = existingPos.PositionName;
            JobDescription = existingPos.JobDescription;
        }

        [RelayCommand]
        private void Save(Window window)
        {
            if (string.IsNullOrWhiteSpace(PosTitle))
            {
                MessageBox.Show("Vui lòng nhập tên chức vụ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}