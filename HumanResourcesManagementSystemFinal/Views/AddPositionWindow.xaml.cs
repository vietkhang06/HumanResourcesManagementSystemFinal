using System.Windows;
using System.Windows.Input;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddPositionWindow : Window
    {
        public string PosTitle { get; private set; }
        public string JobDescription { get; private set; }

        public AddPositionWindow()
        {
            InitializeComponent();
            txtTitle.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Vui lòng nhập tên chức vụ!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTitle.Focus();
                return;
            }

            PosTitle = txtTitle.Text.Trim();
            JobDescription = txtDesc.Text.Trim();

            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }
    }
}   