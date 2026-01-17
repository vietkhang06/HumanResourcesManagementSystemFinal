// Dán class này vào namespace HumanResourcesManagementSystemFinal.ViewModels (hoặc Converters)
using System;
using System.Globalization;
using System.Windows.Data;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public class WidthToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualWidth && double.TryParse(parameter?.ToString(), out double threshold))
            {
                // Nếu chiều rộng > ngưỡng (threshold) -> Hiển thị (Visible)
                // Ngược lại -> Ẩn (Collapsed)
                return actualWidth > threshold ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class WidthToInverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualWidth && double.TryParse(parameter?.ToString(), out double threshold))
            {
                // Ngược lại với cái trên:
                // Nếu chiều rộng > ngưỡng -> Ẩn (Collapsed)
                // Ngược lại -> Hiển thị (Visible)
                return actualWidth > threshold ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}