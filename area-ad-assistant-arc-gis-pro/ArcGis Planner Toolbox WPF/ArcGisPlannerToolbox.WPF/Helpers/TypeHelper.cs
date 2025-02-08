using ArcGIS.Core.Data.DDL;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Helpers;

public static class TypeHelper
{
    public static List<FieldDescription> DictionaryToFieldDescriptionList(Dictionary<string, string> dictionary)
    {
        var list = new List<FieldDescription>();
        foreach (var field in dictionary)
        {
            var item = field.Value switch
            {
                "string" => new FieldDescription(field.Key, ArcGIS.Core.Data.FieldType.String),
                "int" => new FieldDescription(field.Key, ArcGIS.Core.Data.FieldType.Integer),
                "double" => new FieldDescription(field.Key, ArcGIS.Core.Data.FieldType.Double),
                "date" => new FieldDescription(field.Key, ArcGIS.Core.Data.FieldType.Date),
                _ => null,
            };
            if (item is not null)
                list.Add(item);
        }
        return list;
    }
}
