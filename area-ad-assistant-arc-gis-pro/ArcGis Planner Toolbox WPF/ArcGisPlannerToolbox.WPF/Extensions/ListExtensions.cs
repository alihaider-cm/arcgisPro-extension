using ArcGisPlannerToolbox.WPF.Data;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ArcGisPlannerToolbox.WPF.Extensions;

public static class ListExtensions
{
    public static List<T> RemoveDuplicatesFromList<T>(this List<T> list, string propertyName)
    {
        var cleanedList = new List<T>();
        if (list != null)
        {
            cleanedList = new List<T>();
            var propertyValues = new List<string>();
            PropertyInfo cleaningProperty;
            cleaningProperty = list.FirstOrDefault()
                                   .GetType()
                                   .GetProperties()
                                   .Where(p => p.Name == propertyName)
                                   .FirstOrDefault();

            foreach (var item in list)
            {
                if (cleaningProperty != null)
                {
                    var value = cleaningProperty.GetValue(item).ToString();
                    if (!propertyValues.Contains(value))
                    {
                        propertyValues.Add(value);
                        cleanedList.Add(item);
                    }
                }

            }
        }
        return cleanedList;
    }

    public static List<PolygonGeometry> ConvertToArcGisPolygons(this List<ArcGisPlannerToolbox.Core.Models.Geometry> geometries)
    {
        var polygons = new List<PolygonGeometry>();
        var objectId = 1;
        foreach (var geometry in geometries)
        {
            var polygonGeometry = new PolygonGeometry(geometry);
            var item = ArcGIS.Core.Geometry.GeometryEngine.Instance.ImportFromWKB(ArcGIS.Core.Geometry.WkbImportFlags.WkbImportNonTrusted, geometry.Geom, ArcGIS.Core.Geometry.SpatialReferences.WGS84);
            var polygon = ArcGIS.Core.Geometry.PolygonBuilderEx.CreatePolygon(item.Extent);
            polygonGeometry.Polygon = polygon;
            polygonGeometry.ObjectId = objectId;
            polygons.Add(polygonGeometry);
            objectId++;
        }
        return polygons;
    }

}
