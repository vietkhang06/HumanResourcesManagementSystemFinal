using System.Windows;
using System.Windows.Controls;
using HumanResourcesManagementSystemFinal.ViewModels;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class ChangePasswordControl : UserControl
    {
        public ChangePasswordControl()
        {
            InitializeComponent();
        }

        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChangePasswordViewModel vm)
            {
                vm.ChangePasswordCommand.Execute(new object[] { txtOldPass, txtNewPass, txtConfirmPass });
            }
        }
    }
}