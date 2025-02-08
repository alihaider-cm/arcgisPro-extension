using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface IMapOperator
{
    CompositeLayer GetGroupLayerByName(Map map, string groupLayerName);
    CompositeLayer GetGroupLayerByName(CompositeLayer compositeLayer, string groupLayerName);
    Task<CompositeLayer> AddGroupLayer(Map map, string groupLayerName);
    Task<CompositeLayer> AddGroupLayer(GroupLayer groupLayer, string groupLayerName);
    FeatureLayer AddNewFeatureLayer(GroupLayer groupLayer, FeatureLayer newFeatureLayer, string featureLayerName);
    Task RemoveFeatureLayerFromGroup(CompositeLayer compositeLayer, string featureLayerName);
    List<MapPoint> GetCoordinatesOfMapView(Envelope envelope);
    Task ZoomToLayer(Layer layer);
    Layer GetSubLayerByName(Map map, string featureLayerName);
    GroupLayer GetGroupLayerOfMember(Map map, string featureLayerName);
    List<FeatureLayer> GetSubLayersOfGroupLayer(GroupLayer groupLayer);
}
