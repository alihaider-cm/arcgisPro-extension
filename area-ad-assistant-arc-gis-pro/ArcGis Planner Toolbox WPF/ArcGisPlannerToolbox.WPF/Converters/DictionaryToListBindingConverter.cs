using ArcGisPlannerToolbox.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ArcGisPlannerToolbox.WPF.Converters;

public class DictionaryToListBindingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Dictionary<Media, bool> mediaDictionary)
        {
            var mediaList = new List<Media>();
            foreach (var item in mediaDictionary)
                mediaList.Add(item.Key);

            return mediaList;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
