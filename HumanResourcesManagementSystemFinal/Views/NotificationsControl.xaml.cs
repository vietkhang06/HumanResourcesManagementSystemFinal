using HumanResourcesManagementSystemFinal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HumanResourcesManagementSystemFinal.Views
{
    /// <summary>
    /// Interaction logic for NotificationsControl.xaml
    /// </summary>
    public partial class NotificationsControl : UserControl
    {
        public NotificationsControl()
        {
            InitializeComponent();
            LoadSampleData();
        }
        private void ListNotifications_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListNotifications.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is NotificationData data)
            {
                NotificationsDetailWindow detailWindow = new NotificationsDetailWindow();

                detailWindow.TxtTitle.Text = data.Title;
                detailWindow.TxtContent.Text = data.Content;
                detailWindow.TxtDate.Text = data.Date.ToString("dd/MM/yyyy");
                detailWindow.TxtType.Text = data.Type;
                detailWindow.TxtDepartment.Text = $"Phòng: {data.Department}";

                if (data.Type == "Khẩn cấp")
                {
                    detailWindow.TxtType.Foreground = System.Windows.Media.Brushes.Red;
                    detailWindow.BrdrType.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(254, 226, 226));
                }

                detailWindow.Show();
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddNotificationWindow();
            if (addWindow.ShowDialog() == true)
            {
                var data = new NotificationData
                {
                    Title = addWindow.NewNotificationTitle,
                    Content = addWindow.NewNotificationContent,
                    Type = (addWindow.CboType.SelectedItem as ComboBoxItem)?.Content.ToString(), // Lấy từ ComboBox
                    Department = (addWindow.CboDepartment.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Date = DateTime.Now
                };

                var newItem = new ListBoxItem
                {
                    Content = data.Title,
                    Tag = data,
                    Padding = new Thickness(10)
                };
                ListNotifications.Items.Add(newItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa không?", "Xác nhận", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (ListNotifications.SelectedItem != null)
                {
                    ListNotifications.Items.Remove(ListNotifications.SelectedItem);
                }
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            DateTime? filterDate = FilterDate.SelectedDate;
            string filterType = (CboFilterType.SelectedItem as ComboBoxItem)?.Content.ToString();
            string filterDept = (CboFilterDepartment.SelectedItem as ComboBoxItem)?.Content.ToString();

            foreach (ListBoxItem item in ListNotifications.Items)
            {
                if (item.Tag is NotificationData data)
                {
                    bool matchType = (filterType == "Chung" || data.Type == filterType);
                    bool matchDept = (filterDept == "Tất cả" || data.Department == filterDept);
                    bool matchDate = (!filterDate.HasValue || data.Date.Date == filterDate.Value.Date);

                    item.Visibility = (matchType && matchDept && matchDate) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            FilterToggleButton.IsChecked = false;
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItem item in ListNotifications.Items)
            {
                item.Visibility = Visibility.Visible;
            }
            FilterToggleButton.IsChecked = false;
        }

        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            if (ListNotifications.SelectedItem is ListBoxItem selectedItem)
            {
                if (selectedItem.Tag is NotificationData data)
                {
                    try
                    {
                        PrintDialog printDlg = new PrintDialog();
                        if (printDlg.ShowDialog() == true)
                        {
                            StackPanel printArea = new StackPanel { Margin = new Thickness(50), Width = 500 };

                            printArea.Children.Add(new TextBlock { Text = "CHI TIẾT THÔNG BÁO", FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 20), HorizontalAlignment = HorizontalAlignment.Center });
                            printArea.Children.Add(new TextBlock { Text = $"Tiêu đề: {data.Title}", FontSize = 14, Margin = new Thickness(0, 5, 0, 5) });
                            printArea.Children.Add(new TextBlock { Text = $"Ngày: {data.Date:dd/MM/yyyy} | Loại: {data.Type}", FontSize = 12, Margin = new Thickness(0, 5, 0, 5) });
                            printArea.Children.Add(new TextBlock { Text = $"Phòng ban: {data.Department}", FontSize = 12, Margin = new Thickness(0, 5, 0, 15) });
                            printArea.Children.Add(new Separator { Margin = new Thickness(0, 0, 0, 15) });
                            printArea.Children.Add(new TextBlock { Text = data.Content, TextWrapping = TextWrapping.Wrap, FontSize = 13 });

                            printDlg.PrintVisual(printArea, "Chi tiết thông báo");

                            MessageBox.Show("Đã xuất PDF thông báo thành công!");
                            FilterToggleButton.IsChecked = false; // Đóng popup
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xuất PDF: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một thông báo trong danh sách trước khi xuất PDF!");
            }
        }
        private void LoadSampleData()
        {
            // Danh sách dữ liệu mẫu bao quát tất cả các trường hợp lọc
            var samples = new List<NotificationData>
    {
        new NotificationData {
            Title = "Thông báo nghỉ lễ Tết 2026",
            Type = "Chung",
            Department = "Nhân sự",
            Date = new DateTime(2026, 1, 15),
            Content = "Toàn thể nhân viên được nghỉ từ ngày 25/01 đến hết ngày 02/02/2026."
        },
        new NotificationData {
            Title = "Lỗi hệ thống máy chủ ERP",
            Type = "Khẩn cấp",
            Department = "Kỹ thuật",
            Date = DateTime.Now,
            Content = "Hệ thống đang bảo trì đột xuất. Đề nghị các phòng ban không truy cập cho đến khi có thông báo mới."
        },
        new NotificationData {
            Title = "Quy định quyết toán thuế quý 1",
            Type = "Chung",
            Department = "Kế toán",
            Date = new DateTime(2026, 1, 10),
            Content = "Yêu cầu các bộ phận nộp hóa đơn chứng từ trước ngày 20 hàng tháng."
        },
        new NotificationData {
            Title = "Họp khẩn tiến độ dự án",
            Type = "Khẩn cấp",
            Department = "Nhân sự",
            Date = DateTime.Now,
            Content = "Họp gấp tại phòng hội nghị tầng 2 vào lúc 14:00 chiều nay."
        },
        new NotificationData {
            Title = "Cấp phát trang thiết bị mới",
            Type = "Chung",
            Department = "Kỹ thuật",
            Date = new DateTime(2026, 1, 18),
            Content = "Danh sách máy tính mới đã về kho, mời các nhân viên có tên đến nhận."
        }
    };

            // Đổ dữ liệu vào ListBox
            ListNotifications.Items.Clear();
            foreach (var data in samples)
            {
                var newItem = new ListBoxItem
                {
                    Content = data.Title, // Hiển thị tiêu đề ra màn hình
                    Tag = data,           // Giấu toàn bộ "hồ sơ" vào Tag để lọc
                    Padding = new Thickness(10),
                    FontSize = 14
                };
                ListNotifications.Items.Add(newItem);
            }
        }
    }
}
