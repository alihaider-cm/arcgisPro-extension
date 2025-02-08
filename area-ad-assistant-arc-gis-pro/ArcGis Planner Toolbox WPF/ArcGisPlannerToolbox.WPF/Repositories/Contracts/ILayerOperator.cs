using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGisPlannerToolbox.Core.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface ILayerOperator
{
    List<FeatureLayer> GetAllFeatureLayersOfSubGroupLayer(CompositeLayer groupLayer);
    Task<List<CompositeLayer>> GetAllSubGroupLayersOfGroupLayer(CompositeLayer groupLayer);
    Task<FeatureClass> GetFeatureClassFromSQLPolygons(List<Geometry> geometries, string fileName, string mediaName);
    Task<FeatureClass> GetFeatureClassFromSQLPolygons(Geometry geometry, string fileName, string mediaName);
    Task<FeatureClass> GetFeatureClassFromPlanning(List<PlanningDataArea> planningDataAreas, string fileName);
    Task<FeatureClass> GetFeatureClassFromAdvertisementAreaGeometry(List<AdvertisementAreaGeometry> advertisementAreaGeometries, string fileName);
    Task<FeatureLayer> CreateFeatureLayer(GroupLayer groupLayer, FeatureClass featureClass);
    Task<bool> CheckIfLayerHasActiveDataBaseConnection(FeatureLayer featureLayer);
    Task ClearLayerSelection(FeatureLayer featureLayer);
    Task SelectMicroZipCode(FeatureLayer featureLayer, string microZipCode);
    Task SelectMicroZipCode(FeatureLayer featureLayer, List<string> microZipCodes);
    Task DeselectMicroZipCode(FeatureLayer featureLayer, string microZipCode);
    Task DeselectMicroZipCode(FeatureLayer featureLayer, List<string> microZipCodes);
    Task SelectFeature(FeatureLayer featureLayer, string fieldName, string value);
    Task DeselectFeature(FeatureLayer featureLayer, string fieldName, string value);
    Task DeselectFeatures(FeatureLayer featureLayer, string query);
    Task DeselectFeatures(FeatureLayer featureLayer);
    Task<FeatureLayer> ConnectLayerToDataBase(FeatureLayer featureLayer, string tableName, string targetFieldName, string joinFieldName, string featureLayerName);
    Task SetRenderer(FeatureLayer featureLayer, CIMColor color, FeatureRendererTarget featureRendererTarget = FeatureRendererTarget.Default, SimpleFillStyle fillStyle = SimpleFillStyle.Solid, bool drawOutline = false, double outlineStroke = 2.0, SimpleLineStyle outlineStyle = SimpleLineStyle.Solid);
    Task SetRenderer(FeatureLayer featureLayer, CIMUniqueValueRenderer uniqueValueRenderer);
    Task<List<string>> GetSelectedFeatureValuesByFieldName(FeatureLayer featureLayer, string fieldName, List<long> selectedFIDList);
    Task<List<ExpandoObject>> GetSelectedFeatureValuesByFieldName(FeatureLayer featureLayer, string query);
    Task<CIMUniqueValueRenderer> CreateUniqueValueRenderer(FeatureLayer featureLayer, List<string> fields);
    Task SelectFeatures(FeatureLayer featureLayer, string fieldName, List<string> features);
    Task SelectFeatures(FeatureLayer featureLayer, string fieldName1, string fieldName2, List<Tuple<string, string>> features);
    Task DeselectFeatures(FeatureLayer featureLayer, string fieldName, List<string> features);
    List<FeatureLayer> GetAllFeatureLayers(CompositeLayer groupLayer);
    Task<List<string>> GetFieldValuesByName(FeatureLayer featureLayer, string fieldName);
    Task<FeatureClass> AddFeaturesToFeatureClass(FeatureClass featureClass, List<Geometry> geometries);
    Task<FeatureClass> CreateFeatureClassFromMemoryDatabase(string fileName, Dictionary<string, string> fields);
    Task<Selection> GetSelectionFromLayer(FeatureLayer featureLayer);
}
