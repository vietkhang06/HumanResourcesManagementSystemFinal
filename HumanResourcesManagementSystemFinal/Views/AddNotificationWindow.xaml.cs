using HumanResourcesManagementSystemFinal.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddNotificationWindow : Window
    {
        public AddNotificationWindow()
        {
            InitializeComponent();
            var vm = new AddNotificationViewModel();
            vm.RequestClose += () => { this.DialogResult = true; this.Close(); };
            this.DataContext = vm;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}