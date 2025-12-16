using System;
using System.Globalization;
using System.Windows.Data;

namespace HumanResourcesManagementSystemFinal.Converters
{
    public class BoolToEyeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible && isVisible)
            {
                return "👁️"; // Icon Mắt Mở (Show)
            }
            return "🔒"; // Icon Khóa (Hide) - Hoặc 👁️/
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}