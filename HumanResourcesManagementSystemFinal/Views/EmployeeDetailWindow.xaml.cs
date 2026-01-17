using System.Windows;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class EmployeeDetailWindow : Window
    {
        public EmployeeDetailWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}