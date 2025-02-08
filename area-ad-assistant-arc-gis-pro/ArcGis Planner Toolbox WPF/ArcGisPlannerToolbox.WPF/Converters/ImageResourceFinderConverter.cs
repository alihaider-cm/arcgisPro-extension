using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ArcGisPlannerToolbox.WPF.Converters;

public class ImageResourceFinderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var path = Directory.GetCurrentDirectory() + "/Resources/Images/Location.png";
        var image = new BitmapImage(new Uri(path));
        return image;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}