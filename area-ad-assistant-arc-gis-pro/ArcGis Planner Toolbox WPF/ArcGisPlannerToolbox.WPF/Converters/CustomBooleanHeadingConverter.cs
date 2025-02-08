using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcGisPlannerToolbox.WPF.Converters;

public class CustomBooleanHeadingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool header)
        {
            if (header)
                return "Geplant";

            return "Ungeplant";
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is string text)
        {
            if(text.Equals("Geplant"))
                return true;
            
            return false;
        }
        return false;
    }
}
public class CustomBooleanHeadingPlannerConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool header)
        {
            if (header)
                return "Digitalisierte Medien";

            return "Nichtdigitalisierte Medien";
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (text.Equals("Digitalisierte Medien"))
                return true;

            return false;
        }
        return false;
    }
}
