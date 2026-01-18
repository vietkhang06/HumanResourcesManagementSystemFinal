using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.Converters
{
    // Class này cần tồn tại để các màn hình cũ không bị lỗi
    public class EmployeeIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Tạm thời trả về ảnh mặc định để không bị lỗi build
            return new BitmapImage(new Uri("pack://application:,,,/Images/default_user.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}