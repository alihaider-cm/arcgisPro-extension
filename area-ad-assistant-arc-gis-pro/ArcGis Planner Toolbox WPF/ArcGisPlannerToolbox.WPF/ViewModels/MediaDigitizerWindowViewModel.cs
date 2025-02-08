using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.Core.Models.DTO;
using ArcGisPlannerToolbox.WPF.Constants.Settings;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class MediaDigitizerWindowViewModel : BindableBase
{
    #region Fields

    private readonly IMapManager _mapManager;
    private readonly ICursorService _cursorService;
    private readonly IMediaRepository _mediaRepository;
    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;
    private readonly IWindowService _windowService;
    private readonly SubscriptionToken _mediaSelectionSubscriptionToken;
    private readonly SubscriptionToken _distributionSelectionSubscriptionToken;

    private bool _notifySelectedMediaTextChange = true;
    private Media _media = null;
    private List<OccupancyUnit> _occupancyUnits = new();
    private List<OccupancyUnit> _selectedOccupancyUnits = new();

    #endregion

    #region Properties

    public ICommand OpenAreaDigitizerWindowCommand { get; set; }
    public ICommand RemoveDigitizedUnitCommand { get; set; }
    public ICommand ClearAllDigitizedUnitCommand { get; set; }
    public ICommand EvaluateCommand { get; set; }
    public ICommand SaveButtonCommand { get; set; }
    public ICommand OpenTourSplitterWindowCommand { get; set; }
    public ICommand OpenUpdateTourCommand { get; set; }

    private string _selectedMedia = "---";
    public string SelectedMedia
    {
        get { return _selectedMedia; }
        set
        {
            _selectedMedia = value;
            OnPropertyChanged();
            if (_notifySelectedMediaTextChange)
                OnSelectedMediaTextChanged();
        }
    }

    private string _printNumber = string.Empty;
    public string PrintNumber
    {
        get { return _printNumber; }
        set { _printNumber = value; OnPropertyChanged(); }
    }

    private string _grossHousholdSum = string.Empty;
    public string GrossHousHoldSum
    {
        get { return _grossHousholdSum; }
        set { _grossHousholdSum = value; OnPropertyChanged(); }
    }

    private string _netHouseholdSum = string.Empty;
    public string NetHouseholdSum
    {
        get { return _netHouseholdSum; }
        set { _netHouseholdSum = value; OnPropertyChanged(); }
    }

    private List<string> _occupancyUnitList = new();
    public List<string> OccupancyUnitList
    {
        get { return _occupancyUnitList; }
        set { _occupancyUnitList = value; OnPropertyChanged(); }
    }

    private string _selectedOccupancyUnit = string.Empty;
    public string SelectedOccupancyUnit
    {
        get { return _selectedOccupancyUnit; }
        set { _selectedOccupancyUnit = value; OnPropertyChanged(); }
    }

    private List<Tour> _tours = new();
    public List<Tour> Tours
    {
        get { return _tours; }
        set { _tours = value; OnPropertyChanged(); }
    }

    private List<Tour> _unDigitizedTours = new();
    public List<Tour> UnDigitizedTours
    {
        get { return _unDigitizedTours; }
        set { _unDigitizedTours = value; OnPropertyChanged(); }
    }

    private List<Tour> _filteredTours = new();
    public List<Tour> FilteredTours
    {
        get { return _filteredTours; }
        set { _filteredTours = value; OnPropertyChanged(); }
    }

    private Tour _selectedFilteredTour = new();
    public Tour SelectedFilteredTour
    {
        get { return _selectedFilteredTour; }
        set { _selectedFilteredTour = value; OnPropertyChanged(); }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get { return _searchText; }
        set { _searchText = value; OnPropertyChanged(); }
    }

    private bool _onlyUnDigitize = false;
    public bool OnlyUnDigitize
    {
        get { return _onlyUnDigitize; }
        set { _onlyUnDigitize = value; OnPropertyChanged(); }
    }

    private bool _showSystemDigitize = false;
    public bool ShowSystemDigitize
    {
        get { return _showSystemDigitize; }
        set { _showSystemDigitize = value; OnPropertyChanged(); }
    }

    private bool _fromMapView;
    public bool FromMapView
    {
        get { return _fromMapView; }
        set { _fromMapView = value; OnPropertyChanged(); }
    }

    private int _toursCount;
    public int ToursCount
    {
        get { return _toursCount; }
        set { _toursCount = value; OnPropertyChanged(); }
    }

    private int _alreadyDigitzedCount;
    public int AlreadyDigitizedCount
    {
        get { return _alreadyDigitzedCount; }
        set { _alreadyDigitzedCount = value; OnPropertyChanged(); }
    }

    private bool _advancedGroupEnable = true;
    public bool AdvancedGroupEnable
    {
        get { return _advancedGroupEnable; }
        set { _advancedGroupEnable = value; OnPropertyChanged(); }
    }

    private bool _digitizeMissingUnitsButton = true;
    public bool DigitizeMissingUnitsButton
    {
        get { return _digitizeMissingUnitsButton; }
        set { _digitizeMissingUnitsButton = value; OnPropertyChanged(); }
    }

    private bool _showSystemDigitizeEnable = true;
    public bool ShowSystemDigitizeEnable
    {
        get { return _showSystemDigitizeEnable; }
        set { _showSystemDigitizeEnable = value; OnPropertyChanged(); }
    }

    private List<Tour> _areas = new();
    public List<Tour> Areas
    {
        get { return _areas; }
        set { _areas = value; OnPropertyChanged(); }
    }

    private List<Tour> _selectedArea = new();
    public List<Tour> SelectedArea
    {
        get { return _selectedArea; }
        set { _selectedArea = value; OnPropertyChanged(); }
    }

    private List<OccupancyUnit> _digitizeUnits = new();
    public List<OccupancyUnit> DigitizeUnits
    {
        get { return _digitizeUnits; }
        set { _digitizeUnits = value; OnPropertyChanged(); }
    }

    private OccupancyUnit _selectedDigitizeUnit = new();
    public OccupancyUnit SelectedDigitizeUnit
    {
        get { return _selectedDigitizeUnit; }
        set { _selectedDigitizeUnit = value; OnPropertyChanged(); }
    }

    #endregion

    public MediaDigitizerWindowViewModel(IMapManager mapManager, ICursorService cursorService,
                                         IMediaRepository mediaRepository,
                                         IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent,
                                         IWindowService windowService)
    {
        _mapManager = mapManager;
        _cursorService = cursorService;
        _windowService = windowService;
        _mediaRepository = mediaRepository;
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;

        OpenAreaDigitizerWindowCommand = new RelayCommand(OpenAreaDigitzerWindow);
        RemoveDigitizedUnitCommand = new RelayCommand(RemoveSelectedItemFromDigitezedUnits);
        ClearAllDigitizedUnitCommand = new RelayCommand(ClearDigitizedUnits);
        EvaluateCommand = new RelayCommand(SaveEvaluatedData);
        SaveButtonCommand = new RelayCommand(OnSave);
        OpenTourSplitterWindowCommand = new RelayCommand(OpenTourSplitterWindow);
        OpenUpdateTourCommand = new RelayCommand(OnUpdateTour);

        _mediaSelectionSubscriptionToken = MediaSelectionChangedEvent.Subscribe(async x => await OnMediaSelectionChanged(x), true);
        _distributionSelectionSubscriptionToken = DistributionSelectionTableChangedEvent.Subscribe(x => UpdateConnectionToDistributionSelectionTable(x));
        TourSplittedEvent.Subscribe(x => OnTourSplitted());
        AreasPublishEvent.Subscribe(x => OnPublishedAreasChanged(x));
    }

    #region Public Methods

    private async Task OnUpdateTour()
    {
        try
        {
            ProApp.Current.MainWindow.Cursor = Cursors.Wait;
            _cursorService.SetCursor(Cursors.Wait);
            
            UpdateTourEvent.Publish(true);
            
            await ExecuteStartupMethods();
            
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
            _cursorService.SetCursor(Cursors.Arrow);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating tour: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async void OnWindowLoaded()
    {
        GetUserSettings();
        await ExecuteStartupMethods();

    }
    public async Task ExecuteStartupMethods()
    {
        await SetupMicroZipCodeLayer();
        if (!SelectedMedia.Equals("---") && string.IsNullOrWhiteSpace(SelectedMedia))
            ClearForm();

        SetSmallestOccUnits();
        SetToursList();
        UpdateMediaLabel();
        UpdateSmallestOccupancyCombobox();

        //TODO: Send Event To TourSpliutterWindow, AreaDigitizerWindow and Reset Area List
        Areas.Clear();
        TourListSelectionChanged.Publish(new Tuple<Tour, List<Tour>>(SelectedFilteredTour, Areas));
        AreaSelectionChangedEvent.Publish(new List<OccupancyUnitDTO>());

    }
    public async void OnFilteredTourSelectionChanged()
    {
        if (SelectedFilteredTour is null) return;

        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        _cursorService.SetCursor(Cursors.Wait);
        //SetCurrentOccupancyUnitOfSelectedTour(); NOTE: not required to implement
        await FillAreasAndResultsList();
        GetOccupancyUnitsOfAllAreas();
        //SetupAreaDigitizerForm();
        //UpdatePrintNumber(); not required
        PrintNumber = Areas.Select(x => x.PrintNumber).Sum().ToString();
        UpdateConnectionToDistributionSelectionTable();
        TourListSelectionChanged.Publish(new Tuple<Tour, List<Tour>>(SelectedFilteredTour, Areas));
        var units = _occupancyUnits.ConvertAll<OccupancyUnitDTO>(x => new(x));
        units.ForEach((x) =>
        {
            if (DigitizeUnits.Any(digitizeUnits => digitizeUnits.ZipCode == x.ZipCode && digitizeUnits.MicroZipCode == x.MicroZipCode))
                x.IsChecked = true;
        });
        AreaSelectionChangedEvent.Publish(units);
        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
        _cursorService.SetCursor(Cursors.Arrow);
    }
    public void OnAreaSelectionChanged(List<object> tours)
    {
        var list = new List<Tour>();
        foreach (var item in tours)
            if (item is Tour tour)
                list.Add(tour);

        SelectedArea = list;
        GetOccupancyUnitsFromSelectedAreas();
        //SetupAreaDigitizerForm();
        var units = _occupancyUnits.ConvertAll<OccupancyUnitDTO>(x => new(x));
        units.ForEach((x) =>
        {
            if (DigitizeUnits.Any(digitizeUnits => digitizeUnits.ZipCode == x.ZipCode && digitizeUnits.MicroZipCode == x.MicroZipCode))
                x.IsChecked = true;
        });
        AreaSelectionChangedEvent.Publish(units);
    }
    public void OnOnlyUndigitizeChecked()
    {
        FilterToursList();
        //FillToursListsListView(FilteredTours); Not Required
        if (OnlyUnDigitize)
            ShowSystemDigitizeEnable = true;
    }
    public void OnShowSystemDigitizeChecked()
    {
        FilterToursList();
    }
    public void OnSearchTextChanged()
    {
        FilterToursList();
    }
    public async void OnWindowClosed()
    {
        MediaSelectionChangedEvent.Unsubscribe(_mediaSelectionSubscriptionToken);
        DistributionSelectionTableChangedEvent.Unsubscribe(_distributionSelectionSubscriptionToken);
        SaveUserSettings();
        await _mapManager.ClearCurrentMicroZipCodeLayerSelection();
        ClearDistributionSelectionTable();
        _geometryRepositoryGebietsassistent.ExecuteStoredProcedureDistributionSelectionTable(-1, string.Empty);
        await _mapManager.ZoomToBestFit();
    }

    #endregion

    #region Private Methods

    private async Task OnMediaSelectionChanged(Media media)
    {
        _media = media;
        await ExecuteStartupMethods();
    }
    private async Task SetupMicroZipCodeLayer()
    {
        if (_mapManager.CheckIfMicroZipCodeLayerExists())
        {
            await _mapManager.SetMircoZipCodeLayer();
            if (!await _mapManager.CheckIfMicroZipCodeLayerHasDBConnection())
            {
                await _mapManager.SetMicroZipCodeLayerVisiblity(false);
                await _mapManager.ConnectMicroZipCodeToDataBase();
            }
        }
    }
    private void ClearForm()
    {
        _notifySelectedMediaTextChange = false;
        SelectedMedia = "---";
        PrintNumber = GrossHousHoldSum = NetHouseholdSum = string.Empty;
        // TODO: Clear all listviews as well
        _notifySelectedMediaTextChange = true;
    }
    private void OnSelectedMediaTextChanged()
    {
        FilterToursList();
        ClearListViews();
        // FillToursListsListView(FilteredTours); Not Required
        ShowSumOfTours();
        ActivateDigitizeMissingUnitsElements();
    }
    private void SetSmallestOccUnits()
    {
        var smallestOccUnitsList = _mediaRepository.GetSmallestOccUnitsList();
        var list = smallestOccUnitsList.OrderBy(u => u.Ordered).Select(u => u.OccupancyUnit).ToList();
        list.Insert(0, "---");
        OccupancyUnitList = list;
        SelectedOccupancyUnit = "---";
    }
    private void SetToursList()
    {
        if (_media is not null)
        {
            Tours = _geometryRepositoryGebietsassistent.GetToursList(_media.Id.ToString(), _media.DistributionAreaSource, false);
            UnDigitizedTours = _geometryRepositoryGebietsassistent.GetToursList(_media.Id.ToString(), _media.DistributionAreaSource, true);
        }
    }
    private void UpdateMediaLabel()
    {
        if (_media is not null)
            SelectedMedia = _media.Name;
    }
    private void UpdateSmallestOccupancyCombobox()
    {
        if (!string.IsNullOrWhiteSpace(_media?.SmallestOccupancyUnit))
            SelectedOccupancyUnit = _media.SmallestOccupancyUnit;
        else
            SelectedOccupancyUnit = "---";
    }
    private void FilterToursList()
    {
        if (_media is not null)
        {
            SetToursList();
            var desiredTours = GetDesiredToursList();
            if (SearchText.Length > 1)
                FilteredTours = desiredTours.Where(t => t.TourName.ToLower().Contains(SearchText.ToLower())).ToList();
            else
                FilteredTours = desiredTours;
        }
    }
    private void ClearListViews()
    {
        DigitizeUnits = new();
        Areas = new();
    }
    private List<Tour> GetDesiredToursList()
    {
        if (OnlyUnDigitize && !ShowSystemDigitize)
            return UnDigitizedTours;
        else if (OnlyUnDigitize && ShowSystemDigitize)
        {
            List<Tour> notDigitizedByHuman = UnDigitizedTours;
            var systemDigitizedTours = Tours.Where(t => t.Author != null && t.Author.Contains("System")).ToList();
            notDigitizedByHuman.AddRange(systemDigitizedTours);
            return notDigitizedByHuman;
        }
        else if (ShowSystemDigitize)
            return Tours.Where(t => t.Author != null && t.Author.Contains("System")).ToList();
        else
            return Tours;
    }
    private void ShowSumOfTours()
    {
        ToursCount = Tours.Count;
        AlreadyDigitizedCount = Tours.Count - UnDigitizedTours.Count;
    }
    private void ActivateDigitizeMissingUnitsElements()
    {
        if (Tours.Count - UnDigitizedTours.Count > 0)
            DigitizeMissingUnitsButton = AdvancedGroupEnable = true;
        else
            DigitizeMissingUnitsButton = AdvancedGroupEnable = false;
    }
    private async Task FillAreasAndResultsList()
    {
        ClearDigitizedUnitsListView(true);
        await _mapManager.ClearCurrentMicroZipCodeLayerSelection();
        try
        {
            Areas = _geometryRepositoryGebietsassistent
                 .GetAreasList(SelectedFilteredTour.OccupancyUnitId.ToString())
                 .Where(x => x.TourName.Equals(SelectedFilteredTour.TourName, System.StringComparison.OrdinalIgnoreCase))
                 .Select(c => { c.OccupancyUnitId = null; return c; })
                 .OrderBy(a => a.Location).ThenBy(a => a.District).ToList();

            DigitizeUnits = _geometryRepositoryGebietsassistent.GetDigitizedOccupancyUnitsList(SelectedFilteredTour.OccupancyUnitId.Value);
            GrossHousHoldSum = DigitizeUnits.Select(x => x.GrossHousehold).Sum().ToString();
            NetHouseholdSum = DigitizeUnits.Select(x => x.NetHouesehold).Sum().ToString();

            if (DigitizeUnits.Count > 0)
            {
                _selectedOccupancyUnits = DigitizeUnits.Distinct().ToList();
                //areaDigitizerForm.SetSelectedOccupancyUnits(digitizedOccupancyUnits);
                //areaDigitizerForm.TryCheckSelectedOccupancyUnits();
            }
            //DigitizeUnits.ForEach(async x => await _mapManager.SelectFeatureOnMicroZipCodeLayer(x.MicroZipCode)); Not Required to implement here
        }
        catch (System.Exception ex)
        {
            return;
        }
    }
    private void ClearDigitizedUnitsListView(bool update)
    {
        DigitizeUnits.Clear();
        if (update)
            UpdateSelectedOccupancyUnits();

    }
    private void UpdateSelectedOccupancyUnits()
    {
        if (_selectedOccupancyUnits.Count == 0)
        {
            var microZipCodes = DigitizeUnits.Select(x => x.MicroZipCode).ToList();
            DigitizeUnits = DigitizeUnits.Where(u => microZipCodes.Contains(u.MicroZipCode)).ToList();
            //areaDigitizerForm.SetSelectedOccupancyUnits(selectedOccupancyUnits);
            //areaDigitizerForm.TryCheckSelectedOccupancyUnits();
        }
    }
    private void GetOccupancyUnitsOfAllAreas()
    {
        var zipCodes = Areas.Select(x => x.ZipCode).ToList();
        _occupancyUnits.Clear();
        foreach (var zipCode in zipCodes)
        {
            var units = _geometryRepositoryGebietsassistent.GetOccupancyUnits(zipCode);
            if (_occupancyUnits.Count == 0)
                _occupancyUnits = units.ToList();
            else
                foreach (var unit in units)
                    if (!_occupancyUnits.Select(o => o.MicroZipCode).ToList().Contains(unit.MicroZipCode))
                        _occupancyUnits.Add(unit);
        }
    }
    private void GetOccupancyUnitsFromSelectedAreas()
    {
        var zipCodes = SelectedArea.Select(x => x.ZipCode).ToList();
        _occupancyUnits.Clear();
        foreach (var zipCode in zipCodes)
        {
            var units = _geometryRepositoryGebietsassistent.GetOccupancyUnits(zipCode);
            if (_occupancyUnits == null)
                _occupancyUnits = units.ToList();
            else
                _occupancyUnits.AddRange(units);
        }
    }
    private void UpdateConnectionToDistributionSelectionTable(int provideNeigborZipCodes = 0)
    {
        try
        {
            if (SelectedFilteredTour is not null)
                _geometryRepositoryGebietsassistent.ExecuteStoredProcedureDistributionSelectionTable(_media.Id, SelectedFilteredTour.Issue, provideNeigborZipCodes);
        }
        catch (System.ArgumentOutOfRangeException)
        {
            return;
        }
    }
    private void ClearDistributionSelectionTable()
    {
        try
        {
            _geometryRepositoryGebietsassistent.ExecuteStoredProcedureDistributionSelectionTable(0, "", 0);
        }
        catch (Exception)
        {
            return;
        }
    }
    private async Task OpenAreaDigitzerWindow()
    {
        if (_media is null)
        {
            MessageBox.Show("Bitte selektieren Sie zuerst ein Medium", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        _cursorService.SetCursor(Cursors.Wait);
        UpdateConnectionToDistributionSelectionTable();
        _mapManager.SetVisiblityParameter(true);
        _mapManager.SetZoomParameter(UserProfile.Current.FilterMediaDockSettings.SelectedZoomOption);
        _mapManager.SetCollapseParameter(UserProfile.Current.FilterMediaDockSettings.CollapsedGroupBox);
        _mapManager.SetAreaReloadability(false);
        await _mapManager.LoadDistributionArea(new List<Media>() { _media }, true);
        _windowService.ShowWindow<AreaDigitizerWindow>();

        var units = _occupancyUnits.ConvertAll<OccupancyUnitDTO>(x => new(x));
        units.ForEach((x) =>
        {
            if (DigitizeUnits.Any(digitizeUnits => digitizeUnits.ZipCode == x.ZipCode && digitizeUnits.MicroZipCode == x.MicroZipCode))
                x.IsChecked = true;
        });
        AreaSelectionChangedEvent.Publish(units);
        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
        _cursorService.SetCursor(Cursors.Arrow);
    }
    private void GetUserSettings()
    {
        OnlyUnDigitize = Properties.Settings.Default.onlyNotDigitized_MediaDigitizerForm;
        ShowSystemDigitize = Properties.Settings.Default.stillShowSystemDigitizedTours_MediaDigitizerForm;
        FromMapView = Properties.Settings.Default.fromMapView_onlyDigitized_MediaDigitizerForm;
    }
    private void SaveUserSettings()
    {
        Properties.Settings.Default.onlyNotDigitized_MediaDigitizerForm = OnlyUnDigitize;
        Properties.Settings.Default.stillShowSystemDigitizedTours_MediaDigitizerForm = ShowSystemDigitize;
        Properties.Settings.Default.fromMapView_onlyDigitized_MediaDigitizerForm = FromMapView;
        Properties.Settings.Default.Save();
    }
    private void RemoveSelectedItemFromDigitezedUnits()
    {
        if (SelectedDigitizeUnit == null) return;
        
        DigitizeUnits.Remove(SelectedDigitizeUnit);
        OnPropertyChanged(nameof(DigitizeUnits));
        
        // Update sums
        GrossHousHoldSum = DigitizeUnits.Select(x => x.GrossHousehold).Sum().ToString();
        NetHouseholdSum = DigitizeUnits.Select(x => x.NetHouesehold).Sum().ToString();
        
        // Notify area digitizer
        var units = _occupancyUnits.ConvertAll<OccupancyUnitDTO>(x => new(x));
        units.ForEach(x => 
        {
            x.IsChecked = DigitizeUnits.Any(digitizeUnits => 
                digitizeUnits.ZipCode == x.ZipCode && 
                digitizeUnits.MicroZipCode == x.MicroZipCode);
        });
        AreaSelectionChangedEvent.Publish(units);
    }
    public void ClearDigitizedUnits()
    {
        // Create a new empty list and assign it to trigger property change notification
        DigitizeUnits = new List<OccupancyUnit>();
        
        // Update sums
        GrossHousHoldSum = "0";
        NetHouseholdSum = "0";
        
        // Clear selected units
        _selectedOccupancyUnits.Clear();
        
        // Notify area digitizer of changes
        var units = _occupancyUnits.ConvertAll<OccupancyUnitDTO>(x => new(x));
        units.ForEach(x => x.IsChecked = false);
        AreaSelectionChangedEvent.Publish(units);
    }
    private void SaveEvaluatedData()
    {
        _selectedOccupancyUnits.ForEach(x =>
        {
            x.EvaluatedFrom = Environment.UserName;
            x.EvaluatedOn = DateTime.Now;
        });
        var occupanyUnitId = SelectedFilteredTour.OccupancyUnitId;
        _geometryRepositoryGebietsassistent.SaveEvaluation(occupanyUnitId.ToString());
        DigitizeUnits = _selectedOccupancyUnits;
    }
    private async Task OnSave()
    {
        string occupancyUnitId = SelectedFilteredTour.OccupancyUnitId.ToString();
        var units = DigitizeUnits;
        if (DigitizeUnits.Count > 0)
        {
            var userName = DigitizeUnits.Select(u => u.Author).First();
            SaveOccupancyUnitsAndTour(occupancyUnitId, userName);
        }
        else if (occupancyUnitId is not null)
            _geometryRepositoryGebietsassistent.DeleteOccupancyUnitsDetail(occupancyUnitId);

        FilterToursList();
        SearchText = string.Empty;

        FilteredTours = _filteredTours;
        ShowSumOfTours();
        await _mapManager.ClearCurrentMicroZipCodeLayerSelection();
        ClearDigitizedUnitsListView(false);
        Areas.Clear();
        try
        {
            SelectFollowingTour();
        }
        catch (ArgumentOutOfRangeException)
        {
            MessageBox.Show("Alle Touren Digitalisiert!", "Digitalisierung", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    public void SaveOccupancyUnitsAndTour(string occupancyUnitId, string userName)
    {
        RemoveDuplicates(DigitizeUnits.Distinct().ToList(), out List<OccupancyUnit> occupancyUnitsWithoutDuplicates);
        _geometryRepositoryGebietsassistent.InsertOccupancyUnitsDetail(occupancyUnitsWithoutDuplicates, occupancyUnitId);
        _geometryRepositoryGebietsassistent.SaveDigitizedOccupancyUnits(occupancyUnitId, userName);
    }
    private void RemoveDuplicates(List<OccupancyUnit> occupancyUnits, out List<OccupancyUnit> output)
    {
        output = new List<OccupancyUnit>();
        foreach (var unit in occupancyUnits)
        {
            var microZipCodes = output.Select(u => u.MicroZipCode).ToList();
            if (!microZipCodes.Contains(unit.MicroZipCode))
            {
                output.Add(unit);
            }
        }
    }
    private void SelectFollowingTour()
    {
        var indexOfCurrentSelection = FilteredTours.IndexOf(SelectedFilteredTour);
        SelectedFilteredTour = FilteredTours[indexOfCurrentSelection + 1];
        OnFilteredTourSelectionChanged();
    }
    private void OpenTourSplitterWindow()
    {
        if (Areas.Count > 0)
        {
            if (!ZeroAreaListExists())
                ShowSplitterTourWindow();
            else
            {
                var dialogResult = MessageBox.Show("Nicht alle Gebiete enthalten eine Auflage", "Split Warnhinweis", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (dialogResult == MessageBoxResult.Yes)
                    ShowSplitterTourWindow();
            }
        }
    }
    private bool ZeroAreaListExists() => Areas.Any(x => x.PrintNumber == 0);
    private void ShowSplitterTourWindow()
    {
        _windowService.ShowWindow<TourSplitterWindow>();
        TourListSelectionChanged.Publish(new Tuple<Tour, List<Tour>>(SelectedFilteredTour, Areas));
    }
    private void OnTourSplitted()
    {
        SearchText = string.Empty;
        FilterToursList();
        ShowSumOfTours();
        ClearDigitizedUnitsListView(false);
        Areas = new();
    }
    private void OnPublishedAreasChanged(List<OccupancyUnitDTO> occupancyUnits)
    {
        //var units = new List<OccupancyUnit>(occupancyUnits);
        //units.AddRange(DigitizeUnits);
        DigitizeUnits = new List<OccupancyUnit>(occupancyUnits);
    }

    #endregion
}
