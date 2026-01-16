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
            if (string.IsNullOrEmpty(empId)) return "/Images/default_user.png"; 
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
            string pathPng = Path.Combine(folder, $"{empId}.png");
            string pathJpg = Path.Combine(folder, $"{empId}.jpg");
            string pathJpeg = Path.Combine(folder, $"{empId}.jpeg");
            string finalPath = null;
            if (File.Exists(pathPng)) finalPath = pathPng;
            else if (File.Exists(pathJpg)) finalPath = pathJpg;
            else if (File.Exists(pathJpeg)) finalPath = pathJpeg;
            if (finalPath != null)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(finalPath);
                    bitmap.EndInit();
                    return bitmap;
                }
                catch
                {
                    return "/Images/default_user.png";
                }
            }
            return "/Images/default_user.png";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}