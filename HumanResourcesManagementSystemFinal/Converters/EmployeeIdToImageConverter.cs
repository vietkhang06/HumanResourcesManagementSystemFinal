using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HumanResourcesManagementSystemFinal.Converters
{
    public class EmployeeIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Images/default_user.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}