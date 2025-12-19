using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HumanResourcesManagementSystemFinal.Converters
{
    public class ActionButtonVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3)
                return Visibility.Collapsed;

            bool isManager = false;
            if (values[0] is bool b)
                isManager = b;

            var currentUserId = values[1]?.ToString();
            var employeeId = values[2]?.ToString();
            if (isManager && !string.IsNullOrEmpty(currentUserId) && !string.IsNullOrEmpty(employeeId) && currentUserId != employeeId)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}