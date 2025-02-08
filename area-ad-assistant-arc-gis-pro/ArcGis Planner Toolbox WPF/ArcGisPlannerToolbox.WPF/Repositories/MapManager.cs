using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Constants;
using ArcGisPlannerToolbox.WPF.Helpers;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the class for the map manager that uses the layer and the map operators to commuicate between forms and the map.
/// </summary>
public class MapManager : IMapManager
{
    #region Fields

    private readonly IGeometryRepositoryGebietsassistent _geometryRepository;
    private readonly ILayerOperator _layerOperator;
    private readonly IMapOperator _mapOperator;
    private readonly ArcGIS.Core.CIM.CIMColor _microZipLayerColor;
    private readonly ArcGIS.Core.CIM.CIMColor _zipLayerColor;

    private CompositeLayer _distributionAreaLayer;
    private CompositeLayer _planningLayer;
    private CompositeLayer _advertisementAreaLayer;
    private FeatureLayer _microZipCodeLayer;
    private FeatureLayer _zipCodeLayer;
    private FeatureLayer _multiBranchLayer;
    private FeatureLayer _conceptLayer;
    private List<Layer> _layers;
    private bool _makeVisible;
    private bool _collapseMedia;
    private bool _areaIsReloadable;
    private double _transparency;
    private string _zoomOption;

    #endregion

    #region Constructor

    public MapManager(ILayerOperator layerOperator, IMapOperator mapOperator, IGeometryRepositoryGebietsassistent geometryRepository)
    {
        _layerOperator = layerOperator;
        _mapOperator = mapOperator;
        _geometryRepository = geometryRepository;
        _microZipLayerColor = ArcGIS.Core.CIM.CIMColor.CreateRGBColor(255, 0, 197);  // Pink Color
        _zipLayerColor = ArcGIS.Core.CIM.CIMColor.CreateRGBColor(255, 0, 0); // Red Color
    }

    #endregion

    #region public methods

    public List<FeatureLayer> GetDistributionAreas()
    {
        var compositeLayer = _mapOperator.GetGroupLayerByName(MapView.Active?.Map, MapParams.GroupLayers.Verbreitungsgebiete);
        return _layerOperator.GetAllFeatureLayersOfSubGroupLayer(compositeLayer);
    }
    public async Task LoadDistributionArea(List<Media> mediaList, bool issuesAsFeatureLayer = false)
    {
        if (!DisributiontLayerIsSet())
            await CreateNecessaryGroupLayer();

        _layers = new List<Layer>();
        foreach (var media in mediaList)
        {
            string mediaLayerName = $"{media.Name} (MID {media.Id})";
            var checkIfMediaLayerIsMissing = await CheckIfMediaLayerIsMissing(mediaLayerName);
            if (media.HasDistributionArea && checkIfMediaLayerIsMissing)
            {
                if (issuesAsFeatureLayer)
                {
                    var mediaLayer = await GetNewDistributionAreaFeatureLayer(media, mediaLayerName);
                    await QueuedTask.Run(() =>
                    {
                        mediaLayer.SetTransparency(_transparency);
                        mediaLayer.SetExpanded(!_collapseMedia);
                    });
                    _layers.Add(mediaLayer);
                }
                else
                {
                    var mediaLayers = await GetNewDistributionAreaGroupLayer(media, mediaLayerName);
                    await QueuedTask.Run(() =>
                    {
                        mediaLayers.SetExpanded(!_collapseMedia);
                    });
                    _layers.Add(mediaLayers);
                }
            }
        }
        await ZoomToDesiredExtend();
    }
    public List<string> writePoints()
    {
        List<string> coordinates = new();
        var points = _mapOperator.GetCoordinatesOfMapView(MapView.Active.Extent);
        foreach (var point in points.SkipLast(1))
        {
            MapPoint geographicPoint = (MapPoint)GeometryEngine.Instance.Project(point, SpatialReferences.WGS84);
            double longitude = geographicPoint.X;
            double latitude = geographicPoint.Y;
            coordinates.Add($"{longitude} {latitude}");
        }
        return coordinates;
    }
    public List<string> GetCoords() => writePoints();
    public void SetVisiblityParameter(bool visibility) => _makeVisible = visibility;
    public void SetZoomParameter(string zoomParameter) => _zoomOption = zoomParameter;
    public void SetCollapseParameter(bool collabsed) => _collapseMedia = collabsed;
    public void SetTransparency(double transparency) => _transparency = transparency;
    public bool DisributiontLayerIsSet()
    {
        if (_distributionAreaLayer is null)
        {
            if (FindLayer(MapParams.GroupLayers.Gebietsassistent, MapParams.GroupLayers.Verbreitungsgebiete))
            {
                _distributionAreaLayer = GetLayerByName<CompositeLayer>(MapParams.GroupLayers.Verbreitungsgebiete);
                return true;
            }
            return false;
        }
        return true;
    }
    public void SetAreaReloadability(bool reloadable) => _areaIsReloadable = reloadable;
    public async Task CreateNecessaryGroupLayer()
    {
        var compositeLayer = GetLayerByName<CompositeLayer>(MapParams.GroupLayers.Gebietsassistent);
        if (compositeLayer == null)
        {
           compositeLayer = await _mapOperator.AddGroupLayer(MapView.Active?.Map, MapParams.GroupLayers.Gebietsassistent);
        }
        _distributionAreaLayer = await _mapOperator.AddGroupLayer((GroupLayer)compositeLayer, MapParams.GroupLayers.Verbreitungsgebiete);
        _planningLayer = await _mapOperator.AddGroupLayer((GroupLayer)compositeLayer, MapParams.GroupLayers.Werbegebietsplanung);
        _advertisementAreaLayer = await _mapOperator.AddGroupLayer((GroupLayer)compositeLayer, MapParams.GroupLayers.Werbegebiete);
    }
    public async Task ZoomToDesiredExtend()
    {
        if (_distributionAreaLayer is not null)
        {
            if (_zoomOption.Equals(ZoomOptions.All))
                await _mapOperator.ZoomToLayer(_distributionAreaLayer);
            else if (_zoomOption.Equals(ZoomOptions.LastLayer) && _layers.Count > 0)
                await _mapOperator.ZoomToLayer(_layers.Last());
        }
    }
    public async Task ZoomToBestFit()
    {
        await _mapOperator.ZoomToLayer(_microZipCodeLayer);
    }
    public bool CheckIfMicroZipCodeLayerExists()
    {
        if ((FeatureLayer)_mapOperator.GetSubLayerByName(MapView.Active.Map, MapParams.FeatureLayers.MPLZ_dynamisch) is not null)
            return true;

        MessageBox.Show("Zur Verwendung des Digitalisierungsmodus bitte das mPLZ_dynamisch-Layer einbinden und die Applikation neu starten.",
                "Verbindungsversuch", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        return false;
    }
    public async Task SetMircoZipCodeLayer()
    {
        _microZipCodeLayer = (FeatureLayer)_mapOperator.GetSubLayerByName(MapView.Active.Map, MapParams.FeatureLayers.MPLZ_dynamisch);
        await MakeLayerVisible(_microZipCodeLayer);
    }
    public async Task<bool> CheckIfMicroZipCodeLayerHasDBConnection()
    {
        if (!await _layerOperator.CheckIfLayerHasActiveDataBaseConnection(_microZipCodeLayer))
            return false;

        return true;
    }
    public async Task SetMicroZipCodeLayerVisiblity(bool isVisible) => await QueuedTask.Run(() => _microZipCodeLayer.SetVisibility(isVisible));
    public async Task ConnectMicroZipCodeToDataBase()
    {
        _microZipCodeLayer = await ConnectLayersToDataBaseAsync(_microZipCodeLayer, MapParams.Database.MPLZ_dynamisch, "PLZ", "PLZ", MapParams.FeatureLayers.MPLZ_dynamisch);
        if (_microZipCodeLayer is not null)
            await _layerOperator.SetRenderer(_microZipCodeLayer, _microZipLayerColor, FeatureRendererTarget.Default, SimpleFillStyle.DiagonalCross, true);
    }
    public async Task ClearCurrentMicroZipCodeLayerSelection()
    {
        await _layerOperator.ClearLayerSelection(_microZipCodeLayer);
    }
    public async Task SelectFeatureOnMicroZipCodeLayer(string microZipCode)
    {
        await _layerOperator.SelectMicroZipCode(_microZipCodeLayer, microZipCode);
    }
    public async Task DeselectFeatureOnMicroZipCodeLayer(string microZipCode)
    {
        await _layerOperator.DeselectMicroZipCode(_microZipCodeLayer, microZipCode);
    }
    public async Task<List<string>> GetSelectedMicroZipCodeFromLayer(List<long> selectedFIDList)
    {
        if (_microZipCodeLayer is null)
            _microZipCodeLayer = (FeatureLayer)_mapOperator.GetSubLayerByName(MapView.Active.Map, MapParams.FeatureLayers.MPLZ_dynamisch);

        var microZipCodes = await _layerOperator.GetSelectedFeatureValuesByFieldName(_microZipCodeLayer, "mPLZ_dynamisch.MPLZ", selectedFIDList);
        return microZipCodes;
    }
    public async Task<List<string>> GetSelectedZipCodeFromLayer(List<long> selectedFIDList)
    {
        if (_zipCodeLayer is null)
            _zipCodeLayer = (FeatureLayer)_mapOperator.GetSubLayerByName(MapView.Active.Map, MapParams.FeatureLayers.PLZ_dynamisch);

        var microZipCodes = await _layerOperator.GetSelectedFeatureValuesByFieldName(_zipCodeLayer, "PLZ_dynamisch.PLZ", selectedFIDList);
        return microZipCodes;
    }
    public async Task SelectManyFeaturesOnMicroZipCodeLayer(List<string> microZipCodes)
    {
        await _layerOperator.SelectMicroZipCode(_microZipCodeLayer, microZipCodes);
    }
    public async Task DeselectManyFeaturesOnMicroZipCodeLayer(List<string> microZipCodes)
    {
        await _layerOperator.DeselectMicroZipCode(_microZipCodeLayer, microZipCodes);
    }
    public async Task ClearCurrentZipCodeLayerSelection()
    {
        await _layerOperator.ClearLayerSelection(_zipCodeLayer);
    }
    public async Task SetZipCodeLayer()
    {
        _zipCodeLayer = (FeatureLayer)_mapOperator.GetSubLayerByName(MapView.Active.Map, MapParams.FeatureLayers.PLZ_dynamisch);
        await MakeLayerVisible(_zipCodeLayer);
    }
    public async Task<bool> CheckIfZipCodeLayerHasDBConnection()
    {
        if (!await _layerOperator.CheckIfLayerHasActiveDataBaseConnection(_zipCodeLayer))
            return false;

        return true;
    }
    public async Task ConnectZipCodeToDataBase()
    {
        // TODO: Update Column name of View in SQL to PLZ from GeoKey then
        // TODO: Replace MapParams.Database.PLZ_dynamisch1 with MapParams.Database.PLZ_dynamisch 
        _zipCodeLayer = await ConnectLayersToDataBaseAsync(_zipCodeLayer, MapParams.Database.PLZ_dynamisch, "PLZ", "PLZ", MapParams.FeatureLayers.PLZ_dynamisch);
        if (_zipCodeLayer is not null)
            await _layerOperator.SetRenderer(_zipCodeLayer, _zipLayerColor, FeatureRendererTarget.Default, SimpleFillStyle.DiagonalCross, true);
    }
    public async Task SetZipCodeLayerVisibility(bool isVisible) => await QueuedTask.Run(() => _zipCodeLayer.SetVisibility(isVisible));
    public bool CheckIfZipCodeLayerExists()
    {
        if ((FeatureLayer)_mapOperator.GetSubLayerByName(MapView.Active.Map, MapParams.FeatureLayers.PLZ_dynamisch) is not null)
            return true;

        MessageBox.Show("Zur Verwendung des Digitalisierungsmodus bitte das PLZ_dynamisch-Layer einbinden und die Applikation neu starten.",
                    "Verbindungsversuch", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        return false;
    }
    public async Task CreateMultiBranchPlanningLayer(List<PlanningDataArea> plannedAreas, string customer, string planningNumber, string branchNumber)
    {
        var compositeLayer = GetLayerByName<CompositeLayer>(MapParams.GroupLayers.Gebietsassistent, MapParams.GroupLayers.Werbegebietsplanung);
        if (compositeLayer is not null)
        {
            await CreateNecessaryGroupLayer();
            compositeLayer = GetLayerByName<CompositeLayer>(MapParams.GroupLayers.Gebietsassistent, MapParams.GroupLayers.Werbegebietsplanung);
        }
        if (_multiBranchLayer is not null)
            await _mapOperator.RemoveFeatureLayerFromGroup(compositeLayer, _multiBranchLayer.Name);

        if (plannedAreas.Count == 0)
            return;

        string layerName = $"{customer}_Planung_{planningNumber}_Filiale_{branchNumber}";
        var featureClass = await _layerOperator.GetFeatureClassFromPlanning(plannedAreas, layerName);
        _multiBranchLayer = await _layerOperator.CreateFeatureLayer((GroupLayer)compositeLayer, featureClass);
        var uniqueValueRenderer = await _layerOperator.CreateUniqueValueRenderer(_multiBranchLayer, new List<string> { "Filial_Nr" });
        await _layerOperator.SetRenderer(_multiBranchLayer, uniqueValueRenderer);
        await QueuedTask.Run(() =>
        {
            _multiBranchLayer.SetTransparency(_transparency);
            _multiBranchLayer.SetSelectable(false);
            compositeLayer.SetVisibility(true);
        });
        await _mapOperator.ZoomToLayer(_multiBranchLayer);
    }
    public async Task SelectFeatureOnZipCodeLayer(string zipCode)
    {
        await _layerOperator.SelectFeature(_zipCodeLayer, "PLZ_dynamisch.PLZ", zipCode);
    }
    public async Task DeselectFeatureOnZipCodeLayer(string zipCode)
    {
        await _layerOperator.DeselectFeature(_zipCodeLayer, "PLZ_dynamisch.PLZ", zipCode);
    }
    public async Task ClearMapTreeView()
    {
        var compositeLayer = GetLayerByName<CompositeLayer>(MapParams.GroupLayers.Werbegebietsplanung);
        if (_conceptLayer is not null)
            await _mapOperator.RemoveFeatureLayerFromGroup(compositeLayer, _conceptLayer.Name);

        if (compositeLayer is not null && _multiBranchLayer is not null)
            await _mapOperator.RemoveFeatureLayerFromGroup(compositeLayer, _multiBranchLayer.Name);
        var customerGroupLayers = await _layerOperator.GetAllSubGroupLayersOfGroupLayer(_advertisementAreaLayer);
        foreach (var groupLayer in customerGroupLayers)
        {
            var featureLayers = _layerOperator.GetAllFeatureLayersOfSubGroupLayer(groupLayer);
            foreach (var featureLayer in featureLayers)
                await _mapOperator.RemoveFeatureLayerFromGroup(groupLayer, featureLayer.Name);
        }
        await _mapOperator.ZoomToLayer(compositeLayer);
    }
    public async Task SelectManyFeaturesOnZipCodeLayer(string fieldName, List<string> zipCodes)
    {
        if (_zipCodeLayer is not null)
            await _layerOperator.SelectFeatures(_zipCodeLayer, fieldName, zipCodes);
    }
    public async Task DeselectManyFeaturesOnZipCodeLayer(string fieldName, List<string> zipCodes)
    {
        if (_zipCodeLayer is not null)
            await _layerOperator.DeselectFeatures(_zipCodeLayer, fieldName, zipCodes);
    }
    public async Task ZoomToZipCodeLayer()
    {
        if (_zipCodeLayer is not null)
            await _mapOperator.ZoomToLayer(_zipCodeLayer);
    }
    public async Task ZoomToMultiBranchLayer()
    {
        if (_multiBranchLayer is not null)
            await _mapOperator.ZoomToLayer(_multiBranchLayer);
    }
    public async Task CreateAdvertisementAreaLayer(List<AdvertisementAreaGeometry> advertisementAreaGeometries, string customerName, string customerBranchName)
    {
        if (_advertisementAreaLayer is null)
            await CreateNecessaryGroupLayer();

        if (advertisementAreaGeometries.Count > 0)
        {
            var customerGroupLayer = await _mapOperator.AddGroupLayer((GroupLayer)_advertisementAreaLayer, customerName);
            //string shapFileName = customerBranchName + "_AdvertisementArea";
            string shapFileName = customerBranchName;
            var featureClass = await _layerOperator.GetFeatureClassFromAdvertisementAreaGeometry(advertisementAreaGeometries, shapFileName);
            var layer = await _layerOperator.CreateFeatureLayer((GroupLayer)customerGroupLayer, featureClass);
            await _mapOperator.ZoomToLayer(layer);
            await QueuedTask.Run(() =>
            {
                layer.SetTransparency(_transparency);
                customerGroupLayer.SetExpanded(false);
            });
        }
    }
    public async Task<List<string>> GetAllMediaNamesFromArcMapTreeView()
    {
        List<string> mediaNames = new List<string>();
        if (_distributionAreaLayer is not null)
        {
            var subGroupLayers = await _layerOperator.GetAllSubGroupLayersOfGroupLayer(_distributionAreaLayer);
            for (int i = 0; i < subGroupLayers.Count(); i++)
            {
                var subLayer = subGroupLayers[i] as Layer;
                mediaNames.Add(subLayer.Name);
            }
        }
        return mediaNames;
    }
    public async Task<List<string>> GetMediaIdsFromCustomerAdvertisementAreas()
    {
        var mediumIds = new List<string>();
        var customerBranchFeatureLayers = await GetCustomerBranchFeatureLayers();
        foreach (var featureLayer in customerBranchFeatureLayers)
        {
            var ids = await _layerOperator.GetFieldValuesByName(featureLayer, "Medium_id");
            mediumIds.AddRange(ids);
        }
        return mediumIds;
    }
    public async Task CreatePlanningLayer(string conceptName, List<Core.Models.Geometry> polygons)
    {
        var compositeLayer = GetLayerByName<CompositeLayer>(MapParams.GroupLayers.Gebietsassistent, MapParams.GroupLayers.Werbegebietsplanung);
        if (_conceptLayer is not null)
            await _mapOperator.RemoveFeatureLayerFromGroup(compositeLayer, _conceptLayer.Name);

        if (_planningLayer is null)
            await CreateNecessaryGroupLayer();

        await QueuedTask.Run(() => compositeLayer.SetVisibility(false));
        FeatureClass featureClass = await _layerOperator.CreateFeatureClassFromMemoryDatabase(conceptName, MapParams.FeatureClass.Columns);
        featureClass = await _layerOperator.AddFeaturesToFeatureClass(featureClass, polygons);
        _conceptLayer = await _layerOperator.CreateFeatureLayer((GroupLayer)_planningLayer, featureClass);
        var uniqueValueRenderer = await _layerOperator.CreateUniqueValueRenderer(_conceptLayer, new List<string> { "Medium" });
        await _layerOperator.SetRenderer(_conceptLayer, uniqueValueRenderer);
        await QueuedTask.Run(() =>
        {
            compositeLayer.SetVisibility(true);
            _conceptLayer.SetSelectable(true);
            _conceptLayer.SetExpanded(true);
            _planningLayer.SetExpanded(true);
            _conceptLayer.SetTransparency(_transparency);
        });
    }
    public async Task SelectManyFeaturesOnPlanningLayer(string fieldName, List<string> occupancyUnitIds)
    {
        await _layerOperator.SelectFeatures(_conceptLayer, fieldName, occupancyUnitIds);
    }
    public async Task SelectManyFeaturesOnPlanningLayer(string fieldName1, string fieldName2, List<Tuple<string, string>> occupancyUnitIds)
    {
        await _layerOperator.SelectFeatures(_conceptLayer, fieldName1, fieldName2, occupancyUnitIds);
    }
    public async Task<List<ExpandoObject>> GetSelectionDataFromConceptLayer(string query)
    {
        return await _layerOperator.GetSelectedFeatureValuesByFieldName(_conceptLayer, query);
    }
    public async Task<Selection> GetSelectionFromPlanningLayer()
    {
        if (_conceptLayer is not null)
            return await _layerOperator.GetSelectionFromLayer(_conceptLayer);

        return null;
    }
    public async Task DeSelectFeaturesOnPlanningLayer(string query)
    {
        await _layerOperator.DeselectFeatures(_conceptLayer, query);
    }
    public async Task DeSelectFeaturesOnPlanningLayer()
    {
        await _layerOperator.DeselectFeatures(_conceptLayer);
    }

    #endregion

    #region private methods

    private async Task<bool> CheckIfMediaLayerIsMissing(string mediaLayerName)
    {
        if (_mapOperator.GetGroupLayerByName(_distributionAreaLayer, mediaLayerName) is null)
            return true;
        else
        {
            if (_areaIsReloadable)
            {
                DialogResult msgBox = MessageBox.Show("Gewähltes Medium ist bereits geladen. Möchten Sie es dennoch erneut laden?", "Ladevorgang Medium", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (msgBox == DialogResult.Yes)
                {
                    await _mapOperator.RemoveFeatureLayerFromGroup(_distributionAreaLayer, mediaLayerName);
                    return true;
                }
            }
            return false;
        }
    }
    private async Task<GroupLayer> GetNewDistributionAreaFeatureLayer(Media media, string mediaLayerName)
    {
        List<Core.Models.Geometry> polygons;
        var issues = media.Area?.Select(i => i.Issue).Distinct().ToList();
        GroupLayer mediaAreaLayer = (GroupLayer)(await _mapOperator.AddGroupLayer((GroupLayer)_distributionAreaLayer, mediaLayerName));
        if (issues?.Count == 0 || issues is null)
            polygons = _geometryRepository.GetMultiplePolygons(media, mediaLayerName, MapParams.MapManager.OccupancyUnit);
        else
            polygons = _geometryRepository.GetMultiplePolygons(media, issues, mediaLayerName, MapParams.MapManager.OccupancyUnit);
        if (polygons.Count > 0)
        {
            var featureClass = (await _layerOperator.GetFeatureClassFromSQLPolygons(polygons, $"{MapParams.FeatureLayers.Ausgabenebene}{StringHelper.GetOnlyNumericValue(mediaLayerName)}", media.Name));
            var polygonLayer = await _layerOperator.CreateFeatureLayer(mediaAreaLayer, featureClass);
            if (polygonLayer is not null)
            {
                await QueuedTask.Run(() =>
                {
                    polygonLayer.SetVisibility(true);
                    polygonLayer.SetTransparency(_transparency);
                });
            }
        }
        await Task.Delay(500);
        await AddDistributionLayerByPlanningLevel(mediaAreaLayer, media, mediaLayerName, $"{MapParams.FeatureLayers.PLZ5_Ebene}{StringHelper.GetOnlyNumericValue(mediaLayerName)}", MapParams.FeatureLayers.PlanningLevel.PLZ);
        await Task.Delay(500);
        await AddDistributionLayerByPlanningLevel(mediaAreaLayer, media, mediaLayerName, $"{MapParams.FeatureLayers.Tour_BBE_Ebene}{StringHelper.GetOnlyNumericValue(mediaLayerName)}", MapParams.FeatureLayers.PlanningLevel.BBE);
        return mediaAreaLayer;
    }
    private async Task AddDistributionLayerByPlanningLevel(GroupLayer mediaAreaLayer, Media media, string mediaLayerName, string featureLayerName, string planningLevel)
    {
        var polygons = _geometryRepository.GetMultiplePolygons(media, mediaLayerName, planningLevel);
        if (polygons.Count > 0)
        {
            var featureClass = (await _layerOperator.GetFeatureClassFromSQLPolygons(polygons, featureLayerName, media.Name));
            var polygonLayer = await _layerOperator.CreateFeatureLayer(mediaAreaLayer, featureClass);
            if (polygonLayer is not null)
            {
                await QueuedTask.Run(() =>
                {
                    mediaAreaLayer.MoveLayer(polygonLayer, mediaAreaLayer.Layers.Count);
                    polygonLayer.SetVisibility(true);
                    polygonLayer.SetTransparency(_transparency);
                });
            }
        }
    }
    private async Task<GroupLayer> GetNewDistributionAreaGroupLayer(Media media, string mediaLayerName)
    {
        string featureLayerName, fileName;

        var issues = media.Area.Where(i => i.Issue != null).Select(i => i.Issue).Distinct().ToList();
        var mediaAreaLayer = (GroupLayer)await _mapOperator.AddGroupLayer((GroupLayer)_distributionAreaLayer, mediaLayerName);
        _geometryRepository.ExecuteStoredProcedureDistributionGeography(media.Id, media.Name);
        if (issues.Count > 0)
        {
            var shapeCount = 0;
            foreach (var issue in issues)
            {
                featureLayerName = $"Ausgabe - {issue} (MID {media.Id})";
                fileName = $"MID_{media.Id}_ShapeFile{shapeCount}";
                var areaCodes = media.Area.Where(i => i.Issue == issue).Select(a => a.ZipCode).ToList();
                var polygon = _geometryRepository.GetSinglePolygon(issue, media.Id, mediaLayerName, media.DistributionAreaSource);
                if (polygon is not null)
                {
                    var featureClass = await _layerOperator.GetFeatureClassFromSQLPolygons(polygon, featureLayerName, media.Name);
                    var polygonLayer = await _layerOperator.CreateFeatureLayer(mediaAreaLayer, featureClass);
                    if (polygonLayer is not null)
                    {
                        await QueuedTask.Run(() =>
                        {
                            polygonLayer.SetVisibility(_makeVisible);
                        });
                    }
                }
                shapeCount++;
            }
        }
        else if (media.Area.Select(z => z.ZipCode).Distinct().ToList().Count > 0)
        {
            fileName = $"MID_{media.Id}_ShapeFile";
            var areaCodes = media.Area.Select(a => a.ZipCode).Distinct().ToList();
            var polygon = _geometryRepository.GetSinglePolygon(media.Id, mediaLayerName, media.DistributionAreaSource);
            if (polygon is not null)
            {
                var featureClass = await _layerOperator.GetFeatureClassFromSQLPolygons(polygon, fileName, media.Name);
                var polygonLayer = await _layerOperator.CreateFeatureLayer(mediaAreaLayer, featureClass);
                if (polygonLayer is not null)
                {
                    await QueuedTask.Run(() =>
                    {
                        polygonLayer.SetVisibility(_makeVisible);
                    });
                }
            }
        }
        return mediaAreaLayer;
    }
    private bool FindLayer(string layerName)
    {
        var alreadyExist = MapView.Active?.Map.Layers.Any(x => x.Name.Equals(layerName));
        if (alreadyExist is null)
            return false;
        else if (alreadyExist.Value)
            return true;

        return false;
    }
    private bool FindLayer(string groupLayerName, string layerName)
    {
        var groupLayer = GetLayerByName<CompositeLayer>(groupLayerName, layerName);
        var alreadyExist = groupLayer.Layers.Any(x => x.Name.Equals(layerName));

        return alreadyExist;
    }
    private async Task MakeLayerVisible(FeatureLayer featureLayer)
    {
        var groupLayerOfMicroZipCodeLayer = _mapOperator.GetGroupLayerOfMember(MapView.Active.Map, featureLayer.Name);
        await QueuedTask.Run(() =>
        {
            featureLayer.SetVisibility(true);
            groupLayerOfMicroZipCodeLayer.SetVisibility(true);
        });
    }
    private T GetLayerByName<T>(string layerName) where T : Layer
    {
        var layer = MapView.Active?.Map.Layers.FirstOrDefault(x => x.Name.Equals(layerName, StringComparison.CurrentCultureIgnoreCase));
        if (layer is null)
            return null;

        return layer as T;
    }
    private T GetLayerByName<T>(string groupLayerName, string layerName) where T : Layer
    {
        var groupLayer = GetLayerByName<CompositeLayer>(groupLayerName);
        var layer = groupLayer.Layers.FirstOrDefault(x => x.Name.Equals(layerName, StringComparison.CurrentCultureIgnoreCase));
        if(layer is null)
        {
            return null;
        }
        return layer as T;
    }
    private async Task<FeatureLayer> ConnectLayersToDataBaseAsync(FeatureLayer layer, string dbTable, string targetFieldName, string joinFieldName, string featureLayerName)
    {
        //return layer;
        return await _layerOperator.ConnectLayerToDataBase(layer, dbTable, targetFieldName, joinFieldName, featureLayerName);
    }
    private async Task<List<FeatureLayer>> GetCustomerBranchFeatureLayers()
    {
        if (_advertisementAreaLayer is not null)
        {
            var customerBranchFeatureLayers = new List<FeatureLayer>();
            var customerGroupLayers = await _layerOperator.GetAllSubGroupLayersOfGroupLayer(_advertisementAreaLayer);
            foreach (var groupLayer in customerGroupLayers)
            {
                var featureLayers = _layerOperator.GetAllFeatureLayers(groupLayer);
                customerBranchFeatureLayers.AddRange(featureLayers);
            }
            return customerBranchFeatureLayers;
        }
        return new List<FeatureLayer>();
    }

    #endregion

}