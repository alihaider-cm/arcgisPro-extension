using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories;

public class MapOperator : IMapOperator
{
    #region Fields

    private readonly ILayerFactory _layerFactory;

    #endregion

    #region Constructor

    public MapOperator()
    {
        _layerFactory = LayerFactory.Instance;
    }

    #endregion

    #region public methods

    public CompositeLayer GetGroupLayerByName(Map map, string groupLayerName)
    {
        var layer = map.Layers.FirstOrDefault(x => x.Name.Contains(groupLayerName, StringComparison.OrdinalIgnoreCase));
        if (layer is not null)
            return layer as CompositeLayer;

        return null;
    }
    public CompositeLayer GetGroupLayerByName(CompositeLayer compositeLayer, string groupLayerName)
    {
        if (compositeLayer is not null)
            foreach (var layer in compositeLayer.Layers)
                if (layer.Name.Contains(groupLayerName, StringComparison.OrdinalIgnoreCase))
                    return layer as CompositeLayer;

        return null;
    }
    public async Task<CompositeLayer> AddGroupLayer(Map map, string groupLayerName)
    {
        var groupLayer = GetGroupLayerByName(map, groupLayerName);
        return await QueuedTask.Run<CompositeLayer>(() =>
        {
            if (groupLayer is null)
            {
                //NOTE: Add Layer at index 0 otherwise there will be visibility issues
                var newGroupLayer = _layerFactory.CreateGroupLayer(map, 0, groupLayerName);
                return (CompositeLayer)newGroupLayer;
            }
            return groupLayer;
        });
    }
    public async Task<CompositeLayer> AddGroupLayer(GroupLayer groupLayer, string groupLayerName)
    {
        CompositeLayer subGroupLayer = GetGroupLayerByName((CompositeLayer)groupLayer, groupLayerName);
        if (subGroupLayer is null)
        {
            return await QueuedTask.Run<CompositeLayer>(() =>
            {
                GroupLayer newSubGroupLayer = _layerFactory.CreateGroupLayer(groupLayer, groupLayer.Layers.Count, groupLayerName);
                return (CompositeLayer)newSubGroupLayer;
            });
        }
        return subGroupLayer;
    }
    public FeatureLayer AddNewFeatureLayer(GroupLayer groupLayer, FeatureLayer newFeatureLayer, string featureLayerName)
    {
        newFeatureLayer.SetName(featureLayerName);
        groupLayer.MoveLayer(newFeatureLayer, groupLayer.Layers.Count);
        return newFeatureLayer;
    }
    public async Task RemoveFeatureLayerFromGroup(CompositeLayer compositeLayer, string featureLayerName)
    {
        if (compositeLayer is null) return;
        for (int i = 0; i < compositeLayer.Layers.Count; i++)
        {
            var layer = compositeLayer.Layers[i];
            if (layer.Name.Equals(featureLayerName, StringComparison.OrdinalIgnoreCase))
            {
                var groupLayer = (GroupLayer)compositeLayer;
                await QueuedTask.Run(() => groupLayer.RemoveLayer(layer));
            }
        }
    }
    public List<MapPoint> GetCoordinatesOfMapView(Envelope extend)
    {
        return GetPoints(extend);
    }
    private List<MapPoint> GetPoints(Envelope extend)
    {
        Polygon polygon = PolygonBuilderEx.CreatePolygon(extend);
        List<MapPoint> mapPoints = new();
        foreach (MapPoint item in polygon.Points)
            mapPoints.Add(item);

        return mapPoints;
    }
    public async Task ZoomToLayer(Layer layer)
    {
        if(layer is null) return; 
        await MapView.Active?.ZoomToAsync(layer);
    }

    public Layer GetSubLayerByName(Map map, string featureLayerName)
    {
        var layer = GetGroupLayerByName(map, featureLayerName) as Layer;
        if (layer is null)
        {
            foreach (var selectedLayer in map.Layers)
            {
                if (selectedLayer is CompositeLayer compositeLayer)
                    foreach (var subLayer in compositeLayer.Layers)
                        if (subLayer.Name.Equals(featureLayerName, StringComparison.OrdinalIgnoreCase))
                            return subLayer;
            }
            return null;
        }
        return layer;
    }
    public GroupLayer GetGroupLayerOfMember(Map map, string featureLayerName)
    {
        foreach (var layer in map.Layers)
            if (layer is CompositeLayer compositeLayer)
                foreach (var subLayer in compositeLayer.Layers)
                    if (subLayer.Name.Equals(featureLayerName, StringComparison.OrdinalIgnoreCase))
                        return layer as GroupLayer;

        return null;
    }
    public List<FeatureLayer> GetSubLayersOfGroupLayer(GroupLayer groupLayer)
    {
        var layers = new List<FeatureLayer>();
        if (groupLayer is not null)
        {
            foreach (FeatureLayer layer in groupLayer.Layers)
                layers.Add(layer);
        }
        return Enumerable.Empty<FeatureLayer>().ToList();
    }

    #endregion

}
