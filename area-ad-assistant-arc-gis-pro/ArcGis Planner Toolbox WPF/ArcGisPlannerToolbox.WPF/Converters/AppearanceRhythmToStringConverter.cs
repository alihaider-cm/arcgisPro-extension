using ArcGisPlannerToolbox.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ArcGisPlannerToolbox.WPF.Converters;

public class AppearanceRhythmToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Media media)
        {
            var appearance = new List<string>();
            if (media.AppersBeginningOfWeek)
                appearance.Add("Wochenanfang");
            if (media.AppersMiddleOfWeek)
                appearance.Add("Wochenmitte");
            if (media.AppersEndOfWeek)
                appearance.Add("Wochenende");

            return string.Join(", ", appearance);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
