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
            // 1. Lấy ID nhân viên từ Binding
            string empId = value as string;
            if (string.IsNullOrEmpty(empId)) return LoadDefault();

            // 2. Tạo đường dẫn tới folder ảnh
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");

            // 3. Quét xem có file nào trùng tên không (ưu tiên png -> jpg -> jpeg)
            string path = "";
            string png = Path.Combine(folder, empId + ".png");
            string jpg = Path.Combine(folder, empId + ".jpg");
            string jpeg = Path.Combine(folder, empId + ".jpeg");

            if (File.Exists(png)) path = png;
            else if (File.Exists(jpg)) path = jpg;
            else if (File.Exists(jpeg)) path = jpeg;

            // Nếu không tìm thấy file -> Trả về ảnh mặc định
            if (string.IsNullOrEmpty(path)) return LoadDefault();

            // 4. LOAD ẢNH TỪ BYTES (QUAN TRỌNG: Để không bị khóa file và cache cũ)
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // Bỏ qua cache cũ
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load xong ngắt kết nối file ngay
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                bitmap.Freeze(); // Tối ưu hiệu năng
                return bitmap;
            }
            catch
            {
                return LoadDefault();
            }
        }

        private object LoadDefault()
        {
            // Đảm bảo bạn có file default_user.png trong thư mục Images của project (Build Action: Resource)
            return new BitmapImage(new Uri("pack://application:,,,/Images/default_user.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}