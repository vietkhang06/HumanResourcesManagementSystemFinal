using System.Windows;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddDepartmentWindow : Window
    {
        public AddDepartmentWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}