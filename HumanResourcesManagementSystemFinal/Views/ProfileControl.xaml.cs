using HumanResourcesManagementSystemFinal.ViewModels;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class ProfileControl : UserControl
    {
        public ProfileControl()
        {
            InitializeComponent();
        }
        public ProfileControl(string employeeId)
        {
            InitializeComponent();
            this.DataContext = new MyProfileViewModel(employeeId);
        }
        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm)
            {
                await vm.LoadUserProfileAsync();
            }
        }
    }
}