using ArcGisPlannerToolbox.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ArcGisPlannerToolbox.WPF.Converters;

public class KeyValueToDictionaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is KeyValuePair<Geometry, string> pair)
        {
            var item = new Dictionary<Geometry, string>();
            item.Add(pair.Key, pair.Value);
            return item;
        }

        return value;
    }
}
