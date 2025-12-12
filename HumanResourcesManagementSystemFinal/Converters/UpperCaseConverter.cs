using System;
using System.Globalization;
using System.Windows.Data;

namespace HumanResourcesManagementSystemFinal.Converters
{
    // Lớp chuyển đổi để chuyển đổi bất kỳ chuỗi nào thành chữ in hoa
    public class UpperCaseConverter : IValueConverter
    {
        // Phương thức chuyển đổi (từ ViewModel sang View)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string input)
            {
                // Chuyển chuỗi thành chữ in hoa
                return input.ToUpper(culture);
            }
            return value;
        }

        // Phương thức chuyển ngược lại (không cần thiết trong trường hợp này)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}