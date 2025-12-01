using System;
using System.Globalization;
using System.Windows.Data;

namespace HumanResourcesManagementSystemFinal.Converters
{
    //Converter đảo ngược (True -> False)
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue) return !booleanValue;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue) return !booleanValue;
            return false;
        }
    }
    //Converter đổi Boolean sang Chữ (True -> "Lưu", False -> "Sửa")
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Parameter truyền vào dạng: "TextKhiTrue|TextKhiFalse"
            string param = parameter as string;
            if (string.IsNullOrEmpty(param)) return value.ToString();

            string[] texts = param.Split('|');
            if (value is bool boolValue && texts.Length == 2)
            {
                return boolValue ? texts[0] : texts[1];
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}