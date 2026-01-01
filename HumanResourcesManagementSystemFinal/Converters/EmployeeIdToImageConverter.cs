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
            string empId = value as string;
            if (string.IsNullOrEmpty(empId)) return "/Images/default_user.png"; // Ảnh mặc định nếu null

            // Đường dẫn thư mục ảnh
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");

            // Kiểm tra các đuôi ảnh phổ biến
            string pathPng = Path.Combine(folder, $"{empId}.png");
            string pathJpg = Path.Combine(folder, $"{empId}.jpg");
            string pathJpeg = Path.Combine(folder, $"{empId}.jpeg");

            string finalPath = "/Images/default_user.png"; // Mặc định

            if (File.Exists(pathPng)) finalPath = pathPng;
            else if (File.Exists(pathJpg)) finalPath = pathJpg;
            else if (File.Exists(pathJpeg)) finalPath = pathJpeg;

            // Load ảnh dưới dạng BitmapImage để tránh khóa file (File Lock)
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(finalPath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return "/Images/default_user.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}