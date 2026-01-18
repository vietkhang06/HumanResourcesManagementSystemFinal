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
            string id = value as string;
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseDir, "Images", "EmployeeImages");
            string defaultUri = "pack://application:,,,/Images/default_user.png";
            string finalPath = null;

            if (!string.IsNullOrEmpty(id))
            {
                string pngPath = Path.Combine(folderPath, $"{id}.png");
                string jpgPath = Path.Combine(folderPath, $"{id}.jpg");

                if (File.Exists(pngPath)) finalPath = pngPath;
                else if (File.Exists(jpgPath)) finalPath = jpgPath;
            }

            if (finalPath == null) return new BitmapImage(new Uri(defaultUri));

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(finalPath, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return new BitmapImage(new Uri(defaultUri));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}