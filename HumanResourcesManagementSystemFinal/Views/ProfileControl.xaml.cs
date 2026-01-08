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
    }
}