using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping.Events;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Constants;
using ArcGisPlannerToolbox.WPF.Constants.Settings;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Repositories;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Startup;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class FilterMediaDockViewModel : DockPane
{
    #region Fields

    private readonly IMediaRepository _mediaRepository;
    private readonly IAppearanceRhythmRepository _appearanceRhythmRepository;
    private readonly IGebietsassistentRepository _gebietsassistentRepository;
    private readonly IMapViewMediaRepository _mapViewMediaRepository;
    private readonly IMapManager _mapManager;

    private readonly SubscriptionToken _tourUpdateSubscriptionToken;

    private bool mapFilterIsActive = false;
    private bool locationFilterIsActive = false;
    private bool searchFilterIsActive = false;
    private bool detailFilterIsActive = false;
    private bool updateNeededByDistributionMedia = false;
    private bool updateNeededByTimeOfAppearance = false;
    private bool updateNeededByAppearanceRhythm = false;
    private bool _selectedTimeOfAppearanceUpdate = true;
    private bool _selectedDistributionAreaUpdate = true;
    private bool _selectedAppearanceRythmUpdate = true;
    private List<Media> _allMediaList = new();

    #endregion

    #region Properties

    public static FilterMediaDockViewModel Instance { get; private set; }

    private string _mediaFilter = string.Empty;
    public string MediaFilter
    {
        get { return _mediaFilter; }
        set { _mediaFilter = value; NotifyPropertyChanged(); }
    }

    private string _locationFilter = string.Empty;
    public string LocationFilter
    {
        get { return _locationFilter; }
        set { _locationFilter = value; NotifyPropertyChanged(); }
    }

    private bool _filterWithinMap = false;
    public bool FilterWithinMap
    {
        get { return _filterWithinMap; }
        set { _filterWithinMap = value; NotifyPropertyChanged(); }
    }

    private Media _selectedMedia = new();
    public Media SelectedMedia
    {
        get { return _selectedMedia; }
        set { _selectedMedia = value; NotifyPropertyChanged(); }
    }

    private List<Media> _selectedMediaList = new();
    public List<Media> SelectedMediaList
    {
        get { return _selectedMediaList; }
        set { _selectedMediaList = value; NotifyPropertyChanged(); }
    }

    private List<Media> _mediaList = new();
    public List<Media> MediaList
    {
        get { return _mediaList; }
        set { _mediaList = value; NotifyPropertyChanged(); }
    }

    private List<string> _distributionMedia = new();
    public List<string> DistributionMedia
    {
        get { return _distributionMedia; }
        set { _distributionMedia = value; NotifyPropertyChanged(); }
    }

    private List<string> _appearanceRythemNames = new();
    public List<string> AppearanceRhthemNames
    {
        get { return _appearanceRythemNames; }
        set { _appearanceRythemNames = value; NotifyPropertyChanged(); }
    }

    private List<string> _timesOfAppearance = new();
    public List<string> TimesOfAppearance
    {
        get { return _timesOfAppearance; }
        set { _timesOfAppearance = value; NotifyPropertyChanged(); }
    }

    private string _selectedTimeOfAppearance = "Alle";
    public string SelectedTimeOfAppearance
    {
        get { return _selectedTimeOfAppearance; }
        set
        {
            _selectedTimeOfAppearance = value;
            NotifyPropertyChanged();
            if (_selectedTimeOfAppearanceUpdate)
                TimesOfAppearance_OnSelectionChanged();
        }
    }

    private string _selectedDistributionArea = "Alle";
    public string SelectedDistributionArea
    {
        get { return _selectedDistributionArea; }
        set
        {
            _selectedDistributionArea = value;
            NotifyPropertyChanged();
            if (_selectedDistributionAreaUpdate)
                DistributionArea_OnSelectionChanged();
        }
    }

    private string _selectedAppearanceRythm = "Alle";
    public string SelectedAppearanceRythm
    {
        get { return _selectedAppearanceRythm; }
        set
        {
            _selectedAppearanceRythm = value;
            NotifyPropertyChanged();
            if (_selectedAppearanceRythmUpdate)
                AppearanceRhythmNames_OnSelectionChanged();
        }
    }

    private bool _mediumTextEnable;
    public bool MediumTextEnable
    {
        get { return _mediumTextEnable; }
        set { _mediumTextEnable = value; NotifyPropertyChanged(); }
    }

    private bool _mediumTextButtonEnable;
    public bool MediumTextButtonEnable
    {
        get { return _mediumTextButtonEnable; }
        set { _mediumTextButtonEnable = value; NotifyPropertyChanged(); }
    }

    private bool _locationTextEnable;
    public bool LocationTextEnable
    {
        get { return _locationTextEnable; }
        set { _locationTextEnable = value; NotifyPropertyChanged(); }
    }

    private bool _locationTextButtonEnable;
    public bool LocationTextButtonEnable
    {
        get { return _locationTextButtonEnable; }
        set { _locationTextButtonEnable = value; NotifyPropertyChanged(); }
    }

    //private bool _mediaAsGroup;
    //public bool MediaAsGroup
    //{
    //    get { return _mediaAsGroup; }
    //    set { _mediaAsGroup = value; NotifyPropertyChanged(); }
    //}

    //private bool _selectionVisibilityBox;
    //public bool SelectionVisibilityBox
    //{
    //    get { return _selectionVisibilityBox; }
    //    set { _selectionVisibilityBox = value; NotifyPropertyChanged(); }
    //}

    private bool _collapsedGroupBox;
    public bool CollapsedGroupBox
    {
        get { return _collapsedGroupBox; }
        set
        {
            _collapsedGroupBox = value;
            NotifyPropertyChanged();
            UserProfile.Current.FilterMediaDockSettings.CollapsedGroupBox = _collapsedGroupBox;
        }
    }

    private bool _showMediaWithDistribution;
    public bool ShowMediaWithDistribution
    {
        get { return _showMediaWithDistribution; }
        set
        {
            _showMediaWithDistribution = value;
            NotifyPropertyChanged();
            UserProfile.Current.FilterMediaDockSettings.ShowMediaWithDistribution = _showMediaWithDistribution;
        }
    }

    //private bool _issuesFromActiveView;
    //public bool IssuesFromActiveView
    //{
    //    get { return _issuesFromActiveView; }
    //    set { _issuesFromActiveView = value; NotifyPropertyChanged(); }
    //}

    private bool _autoMapViewFilter;
    public bool AutoMapViewFilter
    {
        get { return _autoMapViewFilter; }
        set
        {
            _autoMapViewFilter = value;
            NotifyPropertyChanged();
            UserProfile.Current.FilterMediaDockSettings.AutoMapViewFilter = _autoMapViewFilter;
        }
    }

    private string _selectedZoomOption = string.Empty;
    public string SelectedZoomOption
    {
        get { return _selectedZoomOption; }
        set
        {
            _selectedZoomOption = value;
            NotifyPropertyChanged();
            UserProfile.Current.FilterMediaDockSettings.SelectedZoomOption = _selectedZoomOption;
        }
    }

    private List<string> _zoomOptionItems = new();
    public List<string> ZoomOptionItems
    {
        get { return _zoomOptionItems; }
        set { _zoomOptionItems = value; NotifyPropertyChanged(); }
    }

    private double _selectedTransparencyValue;
    public double SelectedTransparencyValues
    {
        get { return _selectedTransparencyValue; }
        set
        {
            _selectedTransparencyValue = value;
            NotifyPropertyChanged();
            _mapManager.SetTransparency(SelectedTransparencyValues);
            UserProfile.Current.FilterMediaDockSettings.SelectedTransparencyValues = _selectedTransparencyValue;
        }
    }


    #endregion

    #region Commands

    public ICommand MediaFilterCommand { get; set; }
    public ICommand LocationFilterCommand { get; set; }
    public ICommand AllFilterCommand { get; set; }

    #endregion

    #region Initialize

    public FilterMediaDockViewModel()
    {
        if (Instance is null)
            Instance = this;

        _mediaRepository = App.Container.Resolve<IMediaRepository>();
        _appearanceRhythmRepository = App.Container.Resolve<IAppearanceRhythmRepository>();
        _gebietsassistentRepository = App.Container.Resolve<IGebietsassistentRepository>();
        _mapViewMediaRepository = App.Container.Resolve<IMapViewMediaRepository>();
        _mapManager = App.Container.Resolve<IMapManager>();
        MediaFilterCommand = new RelayCommand(OnApplyMediaFilterClick);
        AllFilterCommand = new RelayCommand(OnClearFilterClick);
        LocationFilterCommand = new RelayCommand(OnLocationFilterClick);
        _tourUpdateSubscriptionToken = UpdateTourEvent.Subscribe(async x => await OnUpdatTour(x), true);
    }
    protected override async Task InitializeAsync()
    {
        EnableButtonsAndTextBoxes();
        GetUserSettings();
        await FillFilterColumns();
        ProjectClosingEvent.Subscribe(x => OnProjectClose(x));
        MapViewCameraChangedEvent.Subscribe(async x => await OnMapCameraChanged(x));
    }


    #endregion

    #region Public Methods

    internal static void Show()
    {
        DockPane pane = FrameworkApplication.DockPaneManager.Find(DamlConfig.DockPane.FilterMedia);
        if (pane is null)
            return;

        pane.Activate();
    }
    public async void FilterWithinMap_Changed() => await FilterActiveMediaListView();
    public async void DistributionArea_OnSelectionChanged()
    {
        await FilterActiveMediaListView();
    }
    public async void TimesOfAppearance_OnSelectionChanged()
    {
        await FilterActiveMediaListView();
    }
    public async void AppearanceRhythmNames_OnSelectionChanged()
    {
        await FilterActiveMediaListView();
    }
    public async Task DataGrid_OnMouseDoubleClicked(List<object> mediaList)
    {
        var list = new List<Media>();
        foreach (var media in mediaList)
        {
            if (media is Media item)
                list.Add(item);
        }

        SelectedMediaList = list ;
        //await RestrictDistributionAreaByMapView();
        await LoadArea(true);
    }
    public async void Zoonbox_OnSelectionChanged()
    {
        if (_mapManager.DisributiontLayerIsSet() && SelectedZoomOption.Equals("Alle Verbreitungsgebiete"))
        {
            _mapManager.SetZoomParameter(SelectedZoomOption);
            await _mapManager.ZoomToDesiredExtend();
        }
    }
    public async void ShowMediaWithDistribution_OnChecked() => await OnShowMediaWithDistributionStatusChanged();
    public async void ShowMediaWithDistribution_OnUnchecked() => await OnShowMediaWithDistributionStatusChanged();
    public async void SearchMediumOnEnteryKey() => await OnApplyMediaFilterClick();
    public async void SearchLocationOnEnteryKey() => await OnLocationFilterClick();
    public void OnDataGridSelectedItemChanged() => MediaSelectionChangedEvent.Publish(SelectedMedia);
    public void OnDataGridItemClicked() => MediaSelectionChangedEvent.Publish(SelectedMedia);

    public async void OnWindowClosed()
    {
        UpdateTourEvent.Unsubscribe(_tourUpdateSubscriptionToken);
    }

    #endregion

    #region Private Methods
    private async Task OnUpdatTour(bool reloadable)
    {
        await LoadArea(reloadable);
    }

    private async Task FillFilterColumns()
    {
        _allMediaList = await _mediaRepository.GetAllMediaAsync();
        MediaList = _allMediaList
          .Where(x => ShowMediaWithDistribution ? x.HasDistributionArea == true : true)
          .OrderBy(x => x.Id)
          .ToList();
        if (MediaList is not null)
        {
            var appearanceRhthemNames = (await _appearanceRhythmRepository.GetAllMedaRhythmsAsync())
                .Select(x => x.Name)
                .ToList();

            var distributionMedia = (await _gebietsassistentRepository.GetDestributionMediaAsync())
                .Select(x => x.DistMedia)
                .Distinct()
                .ToList();

            var timesOfAppearance = (await _mediaRepository.GetAvailableTimesOfAppearanceAsync(null, null)).ToList();

            ZoomOptionItems = new List<string>() { ZoomOptions.All, ZoomOptions.LastLayer, ZoomOptions.Off };

            appearanceRhthemNames.Insert(0, "Alle");
            distributionMedia.Insert(0, "Alle");
            timesOfAppearance.Insert(0, "Alle");
            AppearanceRhthemNames = appearanceRhthemNames;
            DistributionMedia = distributionMedia;
            TimesOfAppearance = timesOfAppearance;
        }
        else
            MessageBox.Show("Keine Medien gefunden");
    }
    private async Task OnApplyMediaFilterClick()
    {
        await FilterActiveMediaListView();
    }
    private async Task OnClearFilterClick()
    {
        ProApp.Current.MainWindow.Cursor = Cursors.Wait;

        MediaFilter = LocationFilter = string.Empty;
        if (!AutoMapViewFilter)
            mapFilterIsActive = false;

        searchFilterIsActive = locationFilterIsActive = detailFilterIsActive = FilterWithinMap =
        _selectedTimeOfAppearanceUpdate = _selectedAppearanceRythmUpdate = _selectedDistributionAreaUpdate = false;

        SelectedTimeOfAppearance = SelectedAppearanceRythm = SelectedDistributionArea = "Alle";

        await FillFilterColumns();
        //await FilterActiveMediaListView();

        _selectedTimeOfAppearanceUpdate = _selectedAppearanceRythmUpdate = _selectedDistributionAreaUpdate = true;
        //MapViewActivation();
        //SetToolActivationParameter();
        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
    }
    private async Task OnLocationFilterClick()
    {
        locationFilterIsActive = true;
        MapViewActivation();
        await FilterActiveMediaListView();
    }
    private async Task FilterActiveMediaListView()
    {
        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        DisableButtonsAndTextBoxes();
        MediaList = await GetFilteredMedia();
        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
        EnableButtonsAndTextBoxes();
    }
    private void DisableButtonsAndTextBoxes()
    {
        MediumTextEnable = LocationTextEnable = MediumTextButtonEnable = LocationTextButtonEnable = false;
    }
    private void EnableButtonsAndTextBoxes()
    {
        MediumTextEnable = LocationTextEnable = MediumTextButtonEnable = LocationTextButtonEnable = true;
    }
    private async Task<List<Media>> GetFilteredMedia()
    {
        var expression = await MediaFilterPredicate();
        var val = _allMediaList.Where(expression).OrderBy(x => x.Id).ToList();
        return val;
    }
    private async Task<Func<Media, bool>> MediaFilterPredicate()
    {
        Expression<Func<Media, bool>> predicate = null;
        var criteria = new List<Predicate<Media>>();

        if (int.TryParse(MediaFilter, out int mediaId))
        {
            criteria.Add(x => x.Id == mediaId);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(MediaFilter))
                criteria.Add(x => x.Name.Contains(MediaFilter, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(LocationFilter))
        {
            var mediaIdsFromLocation = await GetMediaWithinLocation();
            if (mediaIdsFromLocation is not null && mediaIdsFromLocation.Count > 0)
                criteria.Add(x => mediaIdsFromLocation.Contains(x.Id));
        }
        if (FilterWithinMap)
        {
            var mediaIdsFromMapView = await GetMediaWithinMapView();
            if (mediaIdsFromMapView is not null && mediaIdsFromMapView.Count > 0)
                criteria.Add(x => mediaIdsFromMapView.Contains(x.Id));
        }
        if (ShowMediaWithDistribution)
        {
            criteria.Add(x => x.HasDistributionArea);
        }
        if (!SelectedDistributionArea.Equals("Alle"))
            criteria.Add(x => x.MediaType is not null && x.MediaType.Equals(SelectedDistributionArea, StringComparison.OrdinalIgnoreCase));
        if (!SelectedTimeOfAppearance.Equals("Alle"))
        {
            if (SelectedTimeOfAppearance.Equals("Wochenanfang", StringComparison.OrdinalIgnoreCase))
                criteria.Add(x => x.AppersBeginningOfWeek);
            else if (SelectedTimeOfAppearance.Equals("Wochenmitte", StringComparison.OrdinalIgnoreCase))
                criteria.Add(x => x.AppersMiddleOfWeek);
            else if (SelectedTimeOfAppearance.Equals("Wochenende", StringComparison.OrdinalIgnoreCase))
                criteria.Add(x => x.AppersEndOfWeek);
        }
        if (!SelectedAppearanceRythm.Equals("Alle"))
            criteria.Add(x => x.AppearanceRhythm.Name.Equals(SelectedAppearanceRythm, StringComparison.OrdinalIgnoreCase));

        predicate = x => criteria.All(predicate => predicate(x));
        return predicate.Compile();
    }
    private async Task<List<Media>> FilterByMapView()
    {
        var mediaIdsFromMapView = await GetMediaWithinMapView();

        if (mediaIdsFromMapView != null)
        {
            return _allMediaList.Where(m => mediaIdsFromMapView.Contains(m.Id)).ToList();
        }
        else
        {
            return _allMediaList;
        }
    }
    private async Task<List<int>> GetMediaWithinMapView()
    {
        var coords = _mapManager.GetCoords();
        return await _mapViewMediaRepository.FindMediaIdByMapView(coords);
    }
    private async Task<List<Media>> FilterByLocation()
    {
        var mediaIdsFromLocation = await GetMediaWithinLocation();
        if (mediaIdsFromLocation is not null)
            return _allMediaList.Where(m => mediaIdsFromLocation.Contains(m.Id)).ToList();
        else
            return _allMediaList;
    }
    private async Task<List<int>> GetMediaWithinLocation() => await _mediaRepository.FindMediaIdByLocation(LocationFilter);
    private List<Media> FilterBySerach()
    {
        return StartMediaSearch();
    }
    private List<Media> StartMediaSearch()
    {
        if (MediaFilter.Length > 0)
        {
            int mediaId;
            if (int.TryParse(MediaFilter, out mediaId))
                return _mediaRepository.FindContinuedByID(mediaId);
            else
                return _mediaRepository.FindByNamePattern(MediaFilter);
        }
        return null;
    }
    private List<Media> FilterByDetail()
    {
        List<string> selMediaTypes = null;
        List<string> selSpreadingTypes = null;
        List<string> selTimesOfAppearance = new List<string>();
        List<string> selAppearanceRhythm = new List<string>();

        if (SelectedDistributionArea != "Alle")
        {
            selMediaTypes = GetMediaTypesFromSelectedDistributionMedia(SelectedDistributionArea).ToList();
            selSpreadingTypes = GetSpreadingTypesFromSelectedDistributionMedia(SelectedDistributionArea).ToList();
        }
        else
        {
            selMediaTypes = _mediaRepository.GetAvailableMediaTypes(null, null);
            selSpreadingTypes = _mediaRepository.GetAvailableSpreadingTypes(null, null);
        }

        if (SelectedTimeOfAppearance != "Alle" && SelectedTimeOfAppearance != null)
            selTimesOfAppearance.Add(SelectedTimeOfAppearance);
        else
            selTimesOfAppearance = _mediaRepository.GetAvailableTimesOfAppearance(selMediaTypes, selSpreadingTypes);

        if (SelectedAppearanceRythm != "Alle" && SelectedAppearanceRythm != null)
            selAppearanceRhythm.Add(SelectedAppearanceRythm);
        else
            selAppearanceRhythm = _allMediaList.Select(a => a.AppearanceRhythm.Name).Distinct().ToList();

        var result = _allMediaList.Where(m => (selMediaTypes.Count > 0 ? selMediaTypes.Contains(m.MediaType) : true))
            .Where(s => (selSpreadingTypes.Count > 0 ? selSpreadingTypes.Contains(s.MediaSpreadingType) : true))
            .Where(t => (selTimesOfAppearance.Count > 0 ? (selTimesOfAppearance.Contains(t.AppersBeginningOfWeek ? "Wochenanfang" : null)
                || selTimesOfAppearance.Contains(t.AppersMiddleOfWeek ? "Wochenmitte" : null)
                || selTimesOfAppearance.Contains(t.AppersEndOfWeek ? "Wochenende" : null)) : true))
            .Where(r => (selAppearanceRhythm.Count > 0 ? selAppearanceRhythm.Contains(r.AppearanceRhythm.Name) : true)).ToList();

        return result;
    }
    private List<string> GetMediaTypesFromSelectedDistributionMedia(string distMedia)
    {
        return _gebietsassistentRepository.GetDestributionMedia()
                .Where(d => d.DistMedia == distMedia)
                .Select(m => m.MediaType).ToList();
    }
    private List<string> GetSpreadingTypesFromSelectedDistributionMedia(string distMedia)
    {
        return _gebietsassistentRepository.GetDestributionMedia()
                .Where(d => d.DistMedia == distMedia)
                .Select(m => m.SpreadingType).ToList();
    }
    private List<Media> GetIntersectionOfFilterLists(List<Media> list1, List<Media> list2, List<Media> list3, List<Media> list4)
    {
        //var intersect1 = list1.Intersect(list2).ToList();
        //var intersect2 = intersect1.Intersect(list3).ToList();

        List<Media> intersect1 = new List<Media>();
        List<Media> intersect2 = new List<Media>();
        List<Media> intersect3 = new List<Media>();

        foreach (var elemL1 in list1)
        {
            if (list2 != null)
            {
                foreach (var elemL2 in list2)
                {
                    if (elemL1.Id == elemL2.Id)
                    {
                        intersect1.Add(elemL1);
                    }
                }
            }
            else
            {
                intersect1.Add(elemL1);
            }
        }
        foreach (var elemI1 in intersect1)
        {
            if (list3 != null)
            {
                foreach (var elemL3 in list3)
                {
                    if (elemI1.Id == elemL3.Id)
                    {
                        intersect2.Add(elemI1);
                    }
                }
            }
            else
            {
                intersect2.Add(elemI1);
            }
        }
        foreach (var elemI2 in intersect2)
        {
            if (list4 != null)
            {
                foreach (var elemL4 in list4)
                {
                    if (elemI2.Id == elemL4.Id)
                    {
                        intersect3.Add(elemI2);
                    }
                }
            }
            else
            {
                intersect3.Add(elemI2);
            }
        }

        return intersect3;
    }
    private void CheckForNeedOfListViewUpdate(string selDistMedia, string selTimeOfAppearance, string appearanceRhythm)
    {
        if (selDistMedia is not null)
            updateNeededByDistributionMedia = true;

        if (selTimeOfAppearance is not null)
            updateNeededByTimeOfAppearance = true;

        if (appearanceRhythm is not null)
            updateNeededByAppearanceRhythm = true;
    }
    private async Task RestrictDistributionAreaByMapView()
    {
        //if (IssuesFromActiveView)
        //{
        //    foreach (var mediaSelected in (await GetSelectedMediaList()))
        //    {
        //        var coords = await _mapManager.GetCoords();
        //        var issuesFromMapView = _mapViewMediaRepository.FindIssuesByMapView(coords, mediaSelected.Id);
        //        var isValid = CheckIfIssuesFromMapViewAreValid(issuesFromMapView);
        //        if (isValid)
        //            mediaSelected.Area = mediaSelected.Area.Where(i => issuesFromMapView.Contains(i.Issue)).ToList();
        //    }
        //}
    }
    public async Task LoadArea(bool reloadable)
    {
        //Ausgewähltes Medium abgreifen
        var mediaSelected = await GetSelectedMediaList();
        if (mediaSelected != null)
        {
            // set mouse cursor to a wait cursor
            ProApp.Current.MainWindow.Cursor = Cursors.Wait;
            if (mediaSelected != null)
            {
                if (mediaSelected.Where(m => m.HasDistributionArea == false).ToList().Count > 0)
                {
                    MessageBox.Show("Nicht alle der ausgewählten Gebiete können geladen werden.",
                        "Vorgang für Laden von Verbreitungsgebieten", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //mapManager.UpdateStatusBar();
                //_mapManager.SetVisiblityParameter(SelectionVisibilityBox);
                _mapManager.SetVisiblityParameter(true);
                _mapManager.SetZoomParameter(SelectedZoomOption);
                _mapManager.SetCollapseParameter(CollapsedGroupBox);
                _mapManager.SetAreaReloadability(reloadable);
                //mapManager.RaiseProgressChangeEvent += raiseProgressEvent;
                await _mapManager.LoadDistributionArea(mediaSelected, true);
                // TODO: Register Event
                // _mediaLoadedEM.DoEvent(mediaSelected);
            }
            // reset mouse cursor to original state
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
        }
    }
    private async Task<List<Media>> GetSelectedMediaList()
    {
        if (SelectedMedia is not null)
        {
            var selectedItemsList = GetSelectedListViewItems(SelectedMediaList);
            var selectedMediaList = new List<Media>();
            foreach (var selectedMediaId in selectedItemsList)
            {
                var mediaSelected = await _mediaRepository.GetWithDistributionAreaAsync(selectedMediaId);
                var mediaArea = mediaSelected.Area.Where(d => d.DataSource == mediaSelected.DistributionAreaSource).ToList();
                mediaSelected.Area = mediaArea;
                selectedMediaList.Add(mediaSelected);
            }
            return selectedMediaList;
        }
        return null;
    }
    private List<int> GetSelectedListViewItems(List<Media> mediaList) => mediaList.Select(x => x.Id).ToList();
    private bool CheckIfIssuesFromMapViewAreValid(List<string> issues)
    {
        foreach (var issue in issues)
        {
            if (issue != null && issue != "")
            {
                return true;
            }
        }
        return false;
    }
    private void GetUserSettings()
    {
        if (ZoomOptionItems.Count == 0)
            ZoomOptionItems = new List<string>() { ZoomOptions.All, ZoomOptions.LastLayer, ZoomOptions.Off };
        //SelectionVisibilityBox = Properties.Settings.Default.slectionVisibility_FilterMediaDockWin;
        //IssuesFromActiveView = Properties.Settings.Default.issuesFromActiveView_FilterMediaDockWin;
        UserProfile.Current.FilterMediaDockSettings.AutoMapViewFilter = AutoMapViewFilter = Properties.Settings.Default.activeMapViewFilter_FilterMediaDockWin;
        UserProfile.Current.FilterMediaDockSettings.SelectedZoomOption = SelectedZoomOption = ZoomOptionItems[Properties.Settings.Default.zoomOption_FilterMediaDockWin];
        UserProfile.Current.FilterMediaDockSettings.ShowMediaWithDistribution = ShowMediaWithDistribution = Properties.Settings.Default.showMediaWithDist_FilterMediaDockWin;
        UserProfile.Current.FilterMediaDockSettings.CollapsedGroupBox = CollapsedGroupBox = Properties.Settings.Default.collapsedMedia_FilterMediaDockWin;
        UserProfile.Current.FilterMediaDockSettings.SelectedTransparencyValues = SelectedTransparencyValues = Properties.Settings.Default.transparencyVal_FilterMediaDockWin;
        //MediaAsGroup = Properties.Settings.Default.mediaAsGroup_FilterMediaDockWin;
    }
    private void SaveUserSettings()
    {
        Properties.Settings.Default.activeMapViewFilter_FilterMediaDockWin = AutoMapViewFilter;
        //Properties.Settings.Default.slectionVisibility_FilterMediaDockWin = SelectionVisibilityBox;
        //Properties.Settings.Default.issuesFromActiveView_FilterMediaDockWin = IssuesFromActiveView;
        Properties.Settings.Default.zoomOption_FilterMediaDockWin = ZoomOptionItems.IndexOf(SelectedZoomOption);
        Properties.Settings.Default.showMediaWithDist_FilterMediaDockWin = ShowMediaWithDistribution;
        Properties.Settings.Default.collapsedMedia_FilterMediaDockWin = CollapsedGroupBox;
        Properties.Settings.Default.transparencyVal_FilterMediaDockWin = SelectedTransparencyValues;
        //Properties.Settings.Default.mediaAsGroup_FilterMediaDockWin = MediaAsGroup;

        Properties.Settings.Default.Save();
    }
    private void MapViewActivation()
    {
        if (locationFilterIsActive)
        {
            mapFilterIsActive = false;
            AutoMapViewFilter = false;
            ShowMediaWithDistribution = false;
        }
        else
        {
            AutoMapViewFilter = true;
            ShowMediaWithDistribution = true;
        }
    }
    private Task OnProjectClose(ProjectClosingEventArgs x)
    {
        SaveUserSettings();
        return Task.CompletedTask;
    }
    private async Task OnShowMediaWithDistributionStatusChanged()
    {
        if (_allMediaList is not null)
        {
            _allMediaList = (await _mediaRepository.GetAllMediaAsync())
                            .Where(m => ShowMediaWithDistribution ? m.HasDistributionArea == true : true)
                            .ToList();
            await FilterActiveMediaListView();
        }
    }
    private async Task OnMapCameraChanged(MapViewCameraChangedEventArgs x)
    {
        if (FilterWithinMap && AutoMapViewFilter && ProApp.Current.MainWindow.Cursor != Cursors.Wait)
        {
            await FilterActiveMediaListView();
        }
    }
    #endregion
}
