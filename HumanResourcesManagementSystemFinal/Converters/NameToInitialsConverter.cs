using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace HumanResourcesManagementSystemFinal.Converters
{
    public class NameToInitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fullName && !string.IsNullOrWhiteSpace(fullName))
            {
                var parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) return "";

                // Nếu chỉ có 1 từ (ví dụ: "Admin") -> Lấy 2 chữ đầu "AD"
                if (parts.Length == 1)
                {
                    return parts[0].Length >= 2
                        ? parts[0].Substring(0, 2).ToUpper()
                        : parts[0].ToUpper();
                }

                // Nếu có nhiều từ -> Lấy chữ cái đầu của Họ và Tên (Ví dụ: "Nguyễn Văn A" -> "NA")
                var firstInitial = parts[0][0];
                var lastInitial = parts[parts.Length - 1][0];

                return $"{firstInitial}{lastInitial}".ToUpper();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}