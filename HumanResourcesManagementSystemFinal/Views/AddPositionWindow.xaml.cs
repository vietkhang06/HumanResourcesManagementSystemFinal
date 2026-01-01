using System.Windows;
using System.Windows.Input;
using HumanResourcesManagementSystemFinal.Models;

namespace HumanResourcesManagementSystemFinal.Views
{
    public partial class AddPositionWindow : Window
    {
        private Position _position;
        public string PosTitle { get; private set; }
        public string JobDescription { get; private set; }

        public AddPositionWindow(Position position = null)
        {
            InitializeComponent();
            _position = position;

            if (_position != null)
            {
                // Lưu ý: Nếu trong XAML bạn đặt tên TextBox là txtTitle thì giữ nguyên.
                // Nếu đặt là txtPositionName thì hãy sửa dòng dưới thành: txtPositionName.Text = ...
                txtTitle.Text = _position.PositionName;
                txtDesc.Text = _position.JobDescription;
                this.Title = "Cập nhật chức vụ";
            }
        }

        // --- CÁC HÀM SỰ KIỆN ĐÃ ĐƯỢC SỬA TÊN CHO KHỚP VỚI XAML ---

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Vui lòng nhập tên chức vụ!");
                return;
            }

            PosTitle = txtTitle.Text;
            JobDescription = txtDesc.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}