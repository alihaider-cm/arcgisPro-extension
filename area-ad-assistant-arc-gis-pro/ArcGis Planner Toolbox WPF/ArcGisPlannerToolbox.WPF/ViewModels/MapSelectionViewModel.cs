using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping.Events;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Data;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Extensions;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Views;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static ArcGisPlannerToolbox.WPF.Events.PlanAdvertisementAreaWizardEvent;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class MapSelectionViewModel : WizardPageViewModel
{

    #region Fields

    private readonly IMapManager _mapManager;
    private readonly ICursorService _cursorService;
    private readonly IWindowService _windowService;
    private readonly IMediaRepository _mediaRepository;
    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;

    private Dictionary<string, int> mediaData = new();
    private List<Media> allMedia = new();
    private List<Media> relevantMedia = new();
    private List<Media> newMedia = new();
    private Dictionary<string, List<Geometry>> mediaUnits = new();
    private List<Geometry> polygons = new();
    private string planningLevel = string.Empty;
    private string baseGeometry = string.Empty;
    private List<AdvertisementAreaAreas> _advertisementAreaAreas = new();
    private List<Geometry> _polygons = new();
    private List<PolygonGeometry> _layerPolygons = new();
    private List<ArcGIS.Core.Geometry.Polygon> _selectedPolygons = new();
    private int _selectedNumberOfCopies = 0;
    private Customer _selectedCustomer = new();
    private CustomerBranch _selectedCustomerBranch = new();
    private List<SelectedPlanningLayerFeature> _selectedFeatures = new();
    private List<long> _mapSelectionIds = new();
    private List<PolygonGeometry> _selectedItems = new();

    #endregion

    #region Properties

    private bool _isAutoPlannerButtonEnable;
    public bool IsAutoPlannerButtonEnable
    {
        get { return _isAutoPlannerButtonEnable; }
        set { _isAutoPlannerButtonEnable = value; OnPropertyChanged(); }
    }

    private List<PolygonGeometry> _groupData = new();
    public List<PolygonGeometry> GroupData
    {
        get { return _groupData; }
        set
        {
            _groupData = value;
            OnPropertyChanged();
            OnPageCommit();
        }
    }

    private PolygonGeometry _selectedGroupData = new();
    public PolygonGeometry SelectedGroupData
    {
        get { return _selectedGroupData; }
        set { _selectedGroupData = value; OnPropertyChanged(); }
    }

    private int _targetNumberOfCopies = 0;
    public int TargetNumberOfCopies
    {
        get { return _targetNumberOfCopies; }
        set { _targetNumberOfCopies = value; OnPropertyChanged(); }
    }

    private string _selectedNumberOfCopiesText;
    public string SelectedNumberOfCopiesText
    {
        get { return _selectedNumberOfCopiesText; }
        set { _selectedNumberOfCopiesText = value; OnPropertyChanged(); }
    }

    private string _editionDifferenceText;
    public string EditionDifferenceText
    {
        get { return _editionDifferenceText; }
        set { _editionDifferenceText = value; OnPropertyChanged(); }
    }

    private bool _mindNeigbourBranches = false;
    public bool MindNeigbourBranches
    {
        get { return _mindNeigbourBranches; }
        set { _mindNeigbourBranches = value; OnPropertyChanged(); }
    }

    public ICommand DeselectSelectedTourCommand { get; set; }
    public ICommand DeselectAllToursCommand { get; set; }
    public ICommand AutoPlannerCommand { get; set; }


    #endregion

    #region Init

    public MapSelectionViewModel(ICursorService cursorService, IMapManager mapManager, IMediaRepository mediaRepository, IWindowService windowService,
                                 IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent)
    {
        _mapManager = mapManager;
        _cursorService = cursorService;
        _windowService = windowService;
        _mediaRepository = mediaRepository;
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;
        PlanningLevelChanged.Subscribe(x => planningLevel = x);
        BaseGeometryChanged.Subscribe(x => baseGeometry = x);
        MapSelectionChangedEvent.Subscribe(async x => await OnMapSelectionChanged(x));
        PlanningLayerSelectedItemChanged.Subscribe(OnPlanningLayerSelectedItemChanged);
        SelectedCustomerChanged.Subscribe(x => _selectedCustomer = x);
        CustomerBranchChanged.Subscribe(x => _selectedCustomerBranch = x);

        DeselectSelectedTourCommand = new RelayCommand(OnDeselectSelectedTour);
        DeselectAllToursCommand = new RelayCommand(DeselectAllTours);
        AutoPlannerCommand = new RelayCommand(ExecuteAutoPlanner);
    }

    #endregion

    #region Public Methods

    public async void OnLoaded()
    {
        Heading = "Selektieren Sie BBEs in der Karte";
        NextStep = 6;
        AllowNext = true;
        await Initialize();
        //WizardPageChangedEvent.Subscribe(x => BeforePageCommit());
    }
    public async Task Initialize()
    {
        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        _cursorService.SetCursor(Cursors.Wait);
        if (allMedia.Count == 0)
            allMedia = await _mediaRepository.GetAllMediaAsync();
        var layers = await GetTreeViewMedia();
        SetMediaDeta(layers);
        if (mediaData.Count > 0)
        {
            var media = GetMediaFromMediaData();
            SetRelevantMedia(media);
            await SetMediaOccupancyUnits();
            EnableAutoPlanner();
            _mapManager.SetCollapseParameter(true);
            _polygons = GetPolygonsFromMedia(relevantMedia, planningLevel);
            _layerPolygons = _polygons.ConvertToArcGisPolygons();
            await _mapManager.CreatePlanningLayer("Planung", _polygons);
            if (baseGeometry == planningLevel)
                ShowFormerSelection();

            //_mapManager.MakeConceptLayerOnlySelectableLayer();
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
            _cursorService.SetCursor(Cursors.Arrow);
        }
    }

    public void ListViewSelectionChanged(List<object> selection)
    {
        _selectedItems.Clear();
        foreach (PolygonGeometry polygonGeometry in selection)
        {
            _selectedItems.Add(polygonGeometry);
        }
        SelectedOccupancyUnitsChanged.Publish(_selectedItems);
    }

    #endregion

    #region Private Methods

    private async Task<List<string>> GetTreeViewMedia() => await _mapManager.GetAllMediaNamesFromArcMapTreeView();
    private void SetMediaDeta(List<string> layerNames)
    {
        if (mediaData.Count == 0)
            mediaData = ExtractMediaDataFromLayerNames(layerNames);
    }
    private Dictionary<string, int> ExtractMediaDataFromLayerNames(List<string> layerNames)
    {
        Dictionary<string, int> mediaData = new Dictionary<string, int>();

        foreach (var name in layerNames)
        {
            var nameParts = name.Split(new string[] { "(MID " }, StringSplitOptions.None);
            var mediaName = nameParts[0].Substring(0, nameParts[0].Length - 1);
            var mediaId = Convert.ToInt32(nameParts[1].Substring(0, nameParts[1].Length - 1));
            mediaData.Add(mediaName, mediaId);
        }
        return mediaData;
    }
    private List<Media> GetMediaFromMediaData()
    {
        return allMedia.Where(m => mediaData.Values.ToList().Contains(m.Id)).Select(m => m).ToList();
    }
    private void SetRelevantMedia(List<Media> selectedMedia)
    {
        if (relevantMedia == null)
        {
            relevantMedia = selectedMedia;
        }
        else
        {
            relevantMedia.Clear();
            newMedia.Clear();
            foreach (var media in selectedMedia)
            {
                if (newMedia is null)
                    newMedia = new List<Media>();

                relevantMedia.Add(media);
                newMedia.Add(media);
            }
        }
    }
    private async Task SetMediaOccupancyUnits()
    {
        mediaUnits = new Dictionary<string, List<Geometry>>();
        foreach (var media in relevantMedia)
        {
            if (planningLevel == PlanningLevels.Kombiniert.ToString())
                polygons = (await _geometryRepositoryGebietsassistent.GetPlanningLayerPolygonsAsync(media, media.Name)).ToList();
            else
                polygons = (await _geometryRepositoryGebietsassistent.GetMultiplePolygonsAsync(media, media.Name, planningLevel)).ToList();
            //polygons.AddRange(geometryRepository.GetMultiplePolygons(media, media.Name, "PLZ").ToList());
            if (polygons.Count > 0)
            {
                mediaUnits.TryAdd(media.Name, polygons.Select(p =>
                {
                    p.Geom = null;
                    p.GeographyString = null;
                    return p;
                }).ToList());
            }
        }
    }
    private void EnableAutoPlanner()
    {
        if (planningLevel == PlanningLevels.PLZ.ToString() && (mediaData?.Count > 0 || mediaUnits?.Count > 0))
            IsAutoPlannerButtonEnable = true;
    }
    private void ShowFormerSelection()
    {
        List<SelectedPlanningLayerFeature> selectedFeatures = new List<SelectedPlanningLayerFeature>();
        if (baseGeometry == "BBE")
        {
            var cleanedList = _advertisementAreaAreas.RemoveDuplicatesFromList("BBE_ID");
            foreach (var item in cleanedList)
            {
                SelectedPlanningLayerFeature feature = new SelectedPlanningLayerFeature();
                feature.BBE_ID = item.BBE_ID.ToString();
                feature.Medium_ID = item.Medium_ID.ToString();
                selectedFeatures.Add(feature);
            }
            if (selectedFeatures.Count > 0)
            {
                _mapManager.SelectManyFeaturesOnPlanningLayer("BBE_ID", selectedFeatures.Select(x => x.BBE_ID).ToList());
                //selectionChangedEM.DoEvent(selectedFeatures, false);
            }
        }
        else if (baseGeometry == "PLZ")
        {
            var cleanedList = _advertisementAreaAreas.RemoveDuplicatesFromList("PLZ");
            foreach (var item in cleanedList)
            {
                SelectedPlanningLayerFeature feature = new SelectedPlanningLayerFeature();
                feature.Tour = $"PLZ {item.PLZ}";
                feature.Medium_ID = item.Medium_ID.ToString();
                selectedFeatures.Add(feature);
            }
            if (selectedFeatures.Count > 0)
            {
                _mapManager.SelectManyFeaturesOnPlanningLayer("Tour", "Medium_ID", selectedFeatures.Select(x => new Tuple<string, string>(x.Tour, x.Medium_ID)).ToList());
                //selectionChangedEM.DoEvent(selectedFeatures, false); TODO: publish event
            }
        }
    }
    private async Task OnMapSelectionChanged(MapSelectionChangedEventArgs args)
    {
        if (args.IsPointSelection)
        {
            _selectedPolygons.Clear();
            var selectedFIDList = args.Selection.ToDictionary().SelectMany(x => x.Value).ToList();
            if (selectedFIDList.Count > 0)
            {
                string query = string.Empty;
                if (selectedFIDList.Count >= _mapSelectionIds.Count)
                {
                    if (selectedFIDList.Count == _mapSelectionIds.Count)
                    {
                        await OnMapDeselectionChanged(selectedFIDList);
                    }

                    _mapSelectionIds = selectedFIDList;
                    //var query = selectedFIDList.CreateQuery("OBJECTID", false);
                    query = $"OBJECTID = {selectedFIDList.Last()}";
                    var data = await _mapManager.GetSelectionDataFromConceptLayer(query);
                    //var Id = 0;
                    foreach (ExpandoObject item in data)
                    {
                        var shape = item.GetValueOrDefault<ArcGIS.Core.Geometry.Polygon>("Shape");
                        //Id = item.GetValueOrDefault<int>("Id");
                        _selectedPolygons.Add(shape);
                    }
                    var arcGisPolygons = GetOverLappingPolygonsFromArcGis();
                    //arcGisPolygons.Where(x => x.Id == Id)
                    if (arcGisPolygons.Count > 1)
                    {
                        _windowService.ShowWindow<PlanningLayerSelectionWindow>();
                        PlanningLayerSelectionChanged.Publish(arcGisPolygons);
                    }
                    else if (arcGisPolygons.Count == 1)
                    {
                        OnPlanningLayerSelectedItemChanged(arcGisPolygons.First());
                    }
                }
                else
                {
                    await OnMapDeselectionChanged(selectedFIDList);
                }
            }
            else
            {
                await OnMapDeselectionChanged(selectedFIDList);
            }
        }
    }
    private void OnPlanningLayerSelectedItemChanged(PolygonGeometry geometry)
    {
        GroupData.Add(geometry);
        //var list = new List<PolygonGeometry>();
        //GroupData.ForEach(x => list.Add(x));
        //list.Add(geometry);
        //GroupData = list;
        RefreshListView();
        UpdateSelectedNumberOfCopiesLabel();
    }
    private List<Geometry> GetPolygonsFromMedia(List<Media> mediaList, string conceptName)
    {
        var listOfPolygons = new List<Geometry>();
        foreach (var media in mediaList)
        {
            List<Geometry> polygons;
            if (planningLevel == PlanningLevels.Kombiniert.ToString())
                polygons = _geometryRepositoryGebietsassistent.GetPlanningLayerPolygons(media, conceptName);
            else
                polygons = _geometryRepositoryGebietsassistent.GetMultiplePolygons(media, conceptName, planningLevel);

            // TODO: remove duplicates
            if (polygons.Count > 0)
                listOfPolygons.AddRange(polygons);

        }
        return listOfPolygons.Distinct().ToList();
    }
    private List<PolygonGeometry> GetOverLappingPolygonsFromArcGis()
    {
        var overlappingPolygons = new List<PolygonGeometry>();
        //var mapPolygons = _selectedPolygons;
        //var layerPolygons = _polygons.ConvertToArcGisPolygons();
        foreach (var mapPolygon in _selectedPolygons)
        {
            foreach (var layerPolygon in _layerPolygons)
            {
                if (ArcGIS.Core.Geometry.GeometryEngine.Instance.Within(mapPolygon, layerPolygon.Polygon))
                    overlappingPolygons.Add(layerPolygon);
            }
        }
        return overlappingPolygons;
    }
    private void CalcualteSelectedNumberOfCopies()
    {
        _selectedNumberOfCopies = 0;
        GroupData.ForEach(x => _selectedNumberOfCopies += x.NumberOfCopies);
    }
    private void UpdateSelectedNumberOfCopiesLabel()
    {
        CalcualteSelectedNumberOfCopies();
        var elGr = CultureInfo.CreateSpecificCulture("el-Gr");
        SelectedNumberOfCopiesText = "Belegt: " + String.Format(elGr, "{0:0,0}", _selectedNumberOfCopies);
        var editionDifference = TargetNumberOfCopies == 0 ? 0 : TargetNumberOfCopies - _selectedNumberOfCopies;
        EditionDifferenceText = "Differenz: " + String.Format(elGr, "{0:0,0}", editionDifference);
    }
    private async Task OnDeselectSelectedTour()
    {
        if (SelectedGroupData is not null && SelectedGroupData.Id != 0)
        {
            //TODO: Store ObjectID from selection
            string query = $"OBJECTID = {SelectedGroupData.ObjectId}"; // TODO: write query to Deselect based on ObjectID
            await _mapManager.DeSelectFeaturesOnPlanningLayer(query);
            GroupData.Remove(SelectedGroupData);
            RefreshListView();
            UpdateSelectedNumberOfCopiesLabel();
        }
    }
    private async Task DeselectAllTours()
    {
        if (GroupData.Count > 0)
        {
            await _mapManager.DeSelectFeaturesOnPlanningLayer();
            GroupData = null;
            GroupData = new();
            _mapSelectionIds.Clear();
            UpdateSelectedNumberOfCopiesLabel();
        }
    }
    private async Task ExecuteAutoPlanner()
    {
        _cursorService.SetCursor(Cursors.Wait);
        await Task.Run(() => StartAutoPlanner());
        await SelectFeaturesByAutoPlannerResult();
        _cursorService.SetCursor(Cursors.Arrow);
    }
    private void StartAutoPlanner()
    {
        if (TargetNumberOfCopies > 0)
            _geometryRepositoryGebietsassistent.ExecuteAutoPlanner_SingleBranch(_selectedCustomer.Kunden_ID, _selectedCustomerBranch.Filial_Nr,
                MindNeigbourBranches, TargetNumberOfCopies);
        else
            _geometryRepositoryGebietsassistent.ExecuteAutoPlanner_SingleBranch(_selectedCustomer.Kunden_ID, _selectedCustomerBranch.Filial_Nr,
                MindNeigbourBranches);
    }
    private async Task SelectFeaturesByAutoPlannerResult()
    {
        var zipCodes = await _geometryRepositoryGebietsassistent.GetAutoPlannerDataAsync();
        EditZipCodes(zipCodes, out List<string> editedZipCoeds);
        await _mapManager.DeSelectFeaturesOnPlanningLayer();
        await _mapManager.SelectManyFeaturesOnPlanningLayer("Tour", editedZipCoeds);

        AddAutoPlannerResultToSelectedFeatures(editedZipCoeds);
        UpdateSelectedMediaUnits();
        UpdateSelectedNumberOfCopiesLabel();
    }
    private void EditZipCodes(List<string> zipCodes, out List<string> editedZipCodes)
    {
        editedZipCodes = new List<string>();
        foreach (var code in zipCodes)
        {
            editedZipCodes.Add("PLZ " + code);
        }
    }
    private void AddAutoPlannerResultToSelectedFeatures(List<string> tours)
    {
        _selectedFeatures.Clear();
        foreach (var media in relevantMedia)
        {
            if (mediaUnits.ContainsKey(media.Name))
            {
                foreach (var tour in tours)
                {
                    SelectedPlanningLayerFeature feature = new SelectedPlanningLayerFeature()
                    {
                        Tour = tour,
                        Medium_ID = media.Id.ToString()
                    };
                    _selectedFeatures.Add(feature);
                }
            }
        }
    }
    private void UpdateSelectedMediaUnits()
    {
        var polygons = new List<PolygonGeometry>();
        var occupancyUnits = new List<Geometry>();
        foreach (var mediaId in _selectedFeatures.Select(f => f.Medium_ID).Distinct().ToList())
        {
            var mediaName = allMedia
                .Where(m => m.Id == int.Parse(mediaId))
                .Select(m => m.Name)
                .FirstOrDefault();
            var Ids = _selectedFeatures
                .Where(f => f.Medium_ID == mediaId)
                .Select(f => f.Id)
                .ToList();
            var tours = _selectedFeatures
                .Where(f => f.Medium_ID == mediaId)
                .Select(f => f.Tour)
                .ToList();
            var occupancyUnitIds = _selectedFeatures
                .Where(f => f.Medium_ID == mediaId
                && f.BBE_ID != "0")
                .Select(f => f.BBE_ID)
                .ToList();
            var unites = mediaUnits[mediaName]
                .Where(u => Ids.Contains(u.Id.ToString())
                || occupancyUnitIds.Contains(u.OccupancyUnitId.ToString())
                || tours.Contains(u.TourName))
                .ToList();


            occupancyUnits.AddRange(unites);
        }
        foreach (var unit in occupancyUnits)
        {
            var polygon = _layerPolygons.First(x => x.Id == unit.Id);
            if (polygon is not null)
                polygons.Add(polygon);
        }
        GroupData = polygons;
    }
    private void RefreshListView()
    {
        var list = new List<PolygonGeometry>();
        GroupData.ForEach(x => list.Add(x));
        GroupData.Clear();
        GroupData = list;
    }
    private async Task OnMapDeselectionChanged(List<long> selectedFIDList)
    {
        var removedIds = _mapSelectionIds.Except(selectedFIDList).ToList();
        if (removedIds.Count > 0)
        {
            removedIds.ForEach(x =>
            {
                var groupItem = GroupData.FirstOrDefault(y => y.ObjectId == x);
                GroupData.Remove(groupItem);
                _mapSelectionIds.Remove(x);
            });
            string query = removedIds.CreateQuery("OBJECTID", false);
            //string query = $"OBJECTID = {SelectedGroupData.ObjectId}"; // TODO: write query to Deselect based on ObjectID
            await _mapManager.DeSelectFeaturesOnPlanningLayer(query);
            //GroupData.Remove(SelectedGroupData);
            RefreshListView();
            UpdateSelectedNumberOfCopiesLabel();
        }
    }
    private void OnPageCommit()
    {
        if (GroupData is null)
            return;

        if (GroupData.Count == 0)
            AllowNext = false;
        else if (GroupData.Count > 0)
            AllowNext = true;
    }
    private void BeforePageCommit()
    {

    }

    #endregion

}
