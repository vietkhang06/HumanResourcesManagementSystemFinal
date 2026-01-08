using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.Converters
{
    public class EmployeeIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string employeeId = value as string;

            // Đường dẫn thư mục ảnh
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string folder = Path.Combine(baseDir, "Images", "Profiles");

            // Ảnh mặc định nếu không tìm thấy
            string defaultImage = "/Images/default-avatar.png"; // Đảm bảo bạn có ảnh này trong project hoặc dùng đường dẫn online
            // Hoặc trả về null để hiện màu nền fallback trong XAML

            if (string.IsNullOrEmpty(employeeId)) return null;

            // Tìm file ảnh có tên trùng với EmployeeID (hỗ trợ jpg, png, jpeg)
            string[] extensions = { ".jpg", ".png", ".jpeg" };
            string finalPath = null;

            foreach (var ext in extensions)
            {
                string path = Path.Combine(folder, employeeId + ext);
                if (File.Exists(path))
                {
                    finalPath = path;
                    break;
                }
            }

            if (finalPath == null) return null; // Trả về null để UI tự xử lý (hiện vòng tròn màu)

            try
            {
                // [QUAN TRỌNG] Kỹ thuật load ảnh không bị khóa file (Non-locking)
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Tải hết vào RAM rồi đóng file ngay
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // Bỏ qua cache cũ để luôn lấy ảnh mới
                bitmap.UriSource = new Uri(finalPath);
                bitmap.EndInit();
                bitmap.Freeze(); // Tối ưu hiệu năng
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}