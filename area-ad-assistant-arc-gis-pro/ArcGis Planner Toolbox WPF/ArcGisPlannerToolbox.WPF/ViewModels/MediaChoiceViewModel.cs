using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.Core.Models.DTO;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static ArcGisPlannerToolbox.WPF.Events.PlanAdvertisementAreaWizardEvent;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class MediaChoiceViewModel : WizardPageViewModel
{
    #region Fields

    private readonly IMapManager _mapManager;
    private readonly ICursorService _cursorService;
    private readonly IMediaRepository _mediaRepository;
    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;
    private readonly SubscriptionToken _baseGeometryChangedToken;
    private SubscriptionToken _wizardPageToken;

    private List<AdvertisementAreaAreas> _advertisementAreaAreas = new();
    private List<Media> _allMedia = new();
    private List<Media> mediaFromCustomerBranch = new();
    private Dictionary<string, int> _mediaData = new();
    private bool _formerDataAccepted;

    #endregion

    #region Properties

    private List<string> _planningLevels = new();
    public List<string> PlanningLevels
    {
        get { return _planningLevels; }
        set { _planningLevels = value; OnPropertyChanged(); }
    }

    private string _selectedPlanningLevel = "PLZ";
    public string SelectedPlanningLevel
    {
        get { return _selectedPlanningLevel; }
        set { _selectedPlanningLevel = value; OnPropertyChanged(); }
    }

    private bool _loadAllMediaButtonEnable;
    public bool LoadAllMediaButtonEnable
    {
        get { return _loadAllMediaButtonEnable; }
        set { _loadAllMediaButtonEnable = value; OnPropertyChanged(); }
    }

    private Dictionary<Media, bool> _flaggedMediaDictionary = new();
    public Dictionary<Media, bool> FlaggedMediaDictionary
    {
        get { return _flaggedMediaDictionary; }
        set { _flaggedMediaDictionary = value; OnPropertyChanged(); }
    }

    public List<MediaDTO> _flaggedMedia = new();
    public List<MediaDTO> FlaggedMedia
    {
        get { return _flaggedMedia; }
        set { _flaggedMedia = value; OnPropertyChanged(); }
    }

    private List<MediaDTO> _selectedFlaggedMedia = new();
    public List<MediaDTO> SelectedFlaggedMedia
    {
        get { return _selectedFlaggedMedia; }
        set { _selectedFlaggedMedia = value; OnPropertyChanged(); }
    }

    private bool _mediaButtonEnable;
    public bool MediumButtonEnable
    {
        get { return _mediaButtonEnable; }
        set { _mediaButtonEnable = value; OnPropertyChanged(); }
    }

    public ICommand LoadMediaCommand { get; set; }
    public ICommand LoadAllMediaCommand { get; set; }

    #endregion

    public MediaChoiceViewModel(ICursorService cursorService, IMapManager mapManager, IMediaRepository mediaRepository, IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent)
    {
        _mapManager = mapManager;
        _cursorService = cursorService;
        _mediaRepository = mediaRepository;
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;

        PlanningLevels = Enum.GetNames<PlanningLevels>().ToList();

        _baseGeometryChangedToken = BaseGeometryChanged.Subscribe(x =>
        {
            if (PlanningLevels.Contains(x))
                SelectedPlanningLevel = x;
        });
        LoadMediaCommand = new RelayCommand(LoadMedia);
        LoadAllMediaCommand = new RelayCommand(LoadAllMedia);
        // TODO: Subscribe advertisementAreaNumber
    }

    #region Public Methods

    public async void OnLoaded()
    {
        Heading = "Medienwahl";
        NextStep = 5;
        AllowNext = true;
        _wizardPageToken = WizardPageChangedEvent.Subscribe(OnPageCommited);
        await Initialize();
    }

    public async Task Initialize()
    {
        //advertisementAreaMedia_lv
        //TODO: Set Mouse to Waiting

        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        _cursorService.SetCursor(Cursors.Wait);
        if (_allMedia.Count == 0)
            _allMedia = await _mediaRepository.GetAllMediaAsync();
        var layers = await GetTreeViewMedia();
        SetMediaDeta(layers);
        AddMediaFromAdvertisementAreaAreas();
        mediaFromCustomerBranch = await GetMediaFromCustomerAdvertisementAreas();
        if (_formerDataAccepted)
            mediaFromCustomerBranch.AddRange(GetMediaByIds(_mediaData.Select(v => v.Value.ToString()).ToList()));
        FlaggedMediaDictionary = CheckMediaForAvailableOccupancyUnits(mediaFromCustomerBranch);
        DefineLoadDigitizedMediaButtonActivation(FlaggedMediaDictionary);
        //FlaggedMedia = _flaggedMediaDictionary.Select(x => new MediaDTO(x.Key, x.Value)).ToList();

        //TODO: reset mouse cursor to original state
        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
        _cursorService.SetCursor(Cursors.Arrow);
    }

    public void OnFlaggedMediaSelectionChanged()
    {
        if (SelectedFlaggedMedia is not null && SelectedFlaggedMedia.Count > 0)
        {
            MediumButtonEnable = true;
        }
    }
    public async Task LoadMedia()
    {
        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        var selectedMedia = GetSelectedMediaList();
        await _mapManager.LoadDistributionArea(selectedMedia, true);
        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
    }
    public async Task LoadAllMedia()
    {
        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        var flaggedMedia = CheckMediaForAvailableOccupancyUnits(mediaFromCustomerBranch);
        var mediaWithOccupancyUnits = flaggedMedia.Where(i => i.Value)
                                                  .Select(m => m.Key)
                                                  .ToList();
        await _mapManager.LoadDistributionArea(mediaWithOccupancyUnits, true);
        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
    }
    public void OnPlanningLevelSelectionChanged()
    {
        if (string.IsNullOrWhiteSpace(SelectedPlanningLevel))
            PlanningLevelChanged.Publish(SelectedPlanningLevel);
    }

    #endregion

    #region Private Methods

    private async Task<List<string>> GetTreeViewMedia() => await _mapManager.GetAllMediaNamesFromArcMapTreeView();
    private void SetMediaDeta(List<string> layerNames)
    {
        _mediaData = ExtractMediaDataFromLayerNames(layerNames);
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
    private void AddMediaFromAdvertisementAreaAreas()
    {
        if (_advertisementAreaAreas != null)
        {
            var mediaIds = _advertisementAreaAreas.Select(a => a.Medium_ID).Distinct().ToList();
            foreach (int id in mediaIds)
            {
                var mediaName = _allMedia.Where(m => m.Id == id).Select(m => m.Name).FirstOrDefault();
                if (!_mediaData.Keys.Contains(mediaName))
                    _mediaData.Add(mediaName, id);
            }
        }
    }
    private async Task<List<Media>> GetMediaFromCustomerAdvertisementAreas()
    {
        var mediaIds = await _mapManager.GetMediaIdsFromCustomerAdvertisementAreas();
        return GetMediaByIds(mediaIds).ToList();
    }
    private List<Media> GetMediaByIds(List<string> mediaIds)
    {
        var media = new List<Media>();
        foreach (var id in mediaIds)
        {
            var m = _mediaRepository.GetWithDistributionArea(Convert.ToInt32(id));
            if (!media.Contains(m))
                media.Add(m);
        }
        return media;
    }
    private Dictionary<Media, bool> CheckMediaForAvailableOccupancyUnits(List<Media> media)
    {
        Dictionary<Media, bool> flaggedMedia = new Dictionary<Media, bool>();
        RemoveDuplicates(media, out List<Media> cleanedMedia);
        foreach (var m in cleanedMedia)
        {
            var medWithArea = _mediaRepository.GetWithDistributionArea(m.Id);
            var geoms = _geometryRepositoryGebietsassistent.GetMultiplePolygons(m, m.Name, "BBE");
            if (geoms.Count > 0)
            {
                flaggedMedia.Add(medWithArea, true);
            }
            else
            {
                flaggedMedia.Add(medWithArea, false);
            }
        }
        return flaggedMedia;
    }
    private void RemoveDuplicates(List<Media> media, out List<Media> mediaWithoutDuplicates)
    {
        mediaWithoutDuplicates = new List<Media>();
        foreach (var med in media)
        {
            if (med != null)
            {
                var currentId = med.Id;
                var storedIds = mediaWithoutDuplicates.Select(m => m.Id).ToList();
                if (!storedIds.Contains(currentId))
                {
                    mediaWithoutDuplicates.Add(med);
                }
            }
        }
    }
    private void DefineLoadDigitizedMediaButtonActivation(IDictionary<Media, bool> flaggedMedia)
    {
        bool digitizedMediaAvailable = false;
        foreach (var media in flaggedMedia)
        {
            if (media.Value == true)
                digitizedMediaAvailable = true;
        }

        if (digitizedMediaAvailable)
            LoadAllMediaButtonEnable = true;
    }
    private List<Media> GetSelectedMediaList()
    {
        var list = new List<Media>();
        foreach (var media in SelectedFlaggedMedia)
            list.Add(media);

        return list;
    }
    public void OnPageCommited(bool args)
    {
        PlanningLevelChanged.Publish(SelectedPlanningLevel);
    }

    #endregion

}
