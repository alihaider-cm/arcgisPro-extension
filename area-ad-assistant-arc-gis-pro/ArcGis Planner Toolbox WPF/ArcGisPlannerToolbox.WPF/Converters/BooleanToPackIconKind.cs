using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArcGisPlannerToolbox.WPF.Converters;

public class BooleanToPackIconKind : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is bool distributedArea)
            return distributedArea ? PackIconKind.CrosshairsGps : PackIconKind.CrosshairsOff;

        return PackIconKind.CrosshairsOff;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
