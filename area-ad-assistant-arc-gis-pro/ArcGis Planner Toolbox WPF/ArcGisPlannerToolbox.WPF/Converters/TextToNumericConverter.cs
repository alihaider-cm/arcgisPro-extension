using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcGisPlannerToolbox.WPF.Converters;

public class TextToNumericConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            else if(int.TryParse(text, out int result))
                return result;

        return 0;
    }
}
