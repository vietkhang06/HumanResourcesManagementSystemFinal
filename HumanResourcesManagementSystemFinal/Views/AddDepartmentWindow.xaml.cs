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

        // Giữ lại chức năng kéo thả cửa sổ vì đây là hành vi giao diện thuần túy
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}