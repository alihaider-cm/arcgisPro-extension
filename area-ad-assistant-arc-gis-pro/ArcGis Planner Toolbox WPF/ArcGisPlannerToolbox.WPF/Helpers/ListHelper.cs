using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ArcGisPlannerToolbox.WPF.Helpers;

public static class ListHelper
{
    public static List<T> FilterGenericListByTextInput<T>(this List<T> list, string input, List<string> propertyNames)
    {
        var filterList = new List<T>();
        if (list.Count > 0)
        {
            input = input.ToLower();
            foreach (var name in propertyNames)
            {
                var result = FilterGenericListByPropertyNameAndInput(list, input, name);
                AddNonExistingItemsToGenericList(filterList, result);
            }
        }
        return filterList;
    }
    public static List<T> FilterGenericListByPropertyNameAndInput<T>(List<T> list, string input, string propertyName)
    {
        var filtered = new List<T>();
        if (list.Count > 0)
        {
            input = input.ToLower();
            filtered = list
                .Where(x => x.GetType()
                            .GetProperties()
                            .Where(p => p.Name.ToLower() == propertyName.ToLower())
                            .FirstOrDefault()
                            .GetValue(x) == null
                            ||
                            x.GetType()
                            .GetProperties()
                            .Where(p => p.Name.ToLower() == propertyName.ToLower())
                            .FirstOrDefault()
                            .GetValue(x)
                            .ToString()
                            .ToLower()
                            .Contains(input))
                .ToList();
        }
        return filtered;
    }
    public static List<string> GetPropertyNames<T>(T item)
    {
        var properties = item
                .GetType()
                .GetProperties()
                .Select(p => p.Name)
                .ToList();
        return properties;
    }
    public static List<T> ToList<T>(this DataTable table) where T : new()
    {
        List<T> list = new List<T>();
        var typeProperties = typeof(T).GetProperties().Select(propertyInfo => new
        {
            PropertyInfo = propertyInfo,
            Type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType
        }).ToList();

        foreach (var row in table.Rows.Cast<DataRow>())
        {
            T obj = new T();
            foreach (var typeProperty in typeProperties)
            {

                if (table.Columns.Contains(typeProperty.PropertyInfo.Name))
                {
                    object value = row[typeProperty.PropertyInfo.Name];
                    object safeValue = value == null || DBNull.Value.Equals(value)
                        ? null
                        : Convert.ChangeType(value, typeProperty.Type);
                    
                    if (safeValue is not null)
                        typeProperty.PropertyInfo.SetValue(obj, safeValue, null);
                }
            }
            list.Add(obj);
        }
        return list;
    }
    private static void AddNonExistingItemsToGenericList<T>(List<T> mainList, List<T> subList)
    {
        foreach (var item in subList)
            if (!mainList.Contains(item))
                mainList.Add(item);
    }

}
