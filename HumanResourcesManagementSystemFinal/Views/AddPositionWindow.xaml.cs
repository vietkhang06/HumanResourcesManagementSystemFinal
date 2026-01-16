using System.Windows;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddPositionWindow : Window
    {
        public AddPositionWindow()
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