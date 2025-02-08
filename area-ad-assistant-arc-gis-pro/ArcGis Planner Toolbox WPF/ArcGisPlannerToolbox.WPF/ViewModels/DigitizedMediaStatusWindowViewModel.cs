using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Helpers;
using ArcGisPlannerToolbox.WPF.Helpers.Excel;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class DigitizedMediaStatusWindowViewModel : BindableBase
{
    private readonly ICursorService _cursorService;
    private readonly IWindowService _windowService;
    private readonly IMediaRepository _mediaRepository;
    private readonly IGebietsassistentRepository _gebietsassistentRepository;
    private readonly IGeometryRepository _geometryRepository;
    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;
    private readonly List<string> _digitisationStateProNames;


    private List<Tour> _updatedAndChangedTours = new();
    private List<Media> _changedMedia = new();
    private List<MediaDigitizationState> _mediaDigitizationStateList = new();

    private List<Tour> _tourList = new();
    public List<Tour> TourList
    {
        get { return _tourList; }
        set { _tourList = value; OnPropertyChanged(); }
    }


    private List<MediaDigitizationState> _mediaDigitizationStates = new();
    public List<MediaDigitizationState> MediaDigitizationStates
    {
        get { return _mediaDigitizationStates; }
        set { _mediaDigitizationStates = value; OnPropertyChanged(); }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get { return _searchText; }
        set { _searchText = value; OnPropertyChanged(); }
    }


    private List<string> _spreadTypes = new();
    public List<string> SpreadTypes
    {
        get { return _spreadTypes; }
        set { _spreadTypes = value; OnPropertyChanged(); }
    }

    private string _selectedSpreadType = "Alle";
    public string SelectedSpreadType
    {
        get { return _selectedSpreadType; }
        set { _selectedSpreadType = value; OnPropertyChanged(); }
    }

    private string _notStartedText = string.Empty;
    public string NotStartedText
    {
        get { return _notStartedText; }
        set { _notStartedText = value; OnPropertyChanged(); }
    }

    private string _finishedText = string.Empty;
    public string FinishedText
    {
        get { return _finishedText; }
        set { _finishedText = value; OnPropertyChanged(); }
    }

    private string _inProcessText = string.Empty;
    public string InProcessText
    {
        get { return _inProcessText; }
        set { _inProcessText = value; OnPropertyChanged(); }
    }

    public ICommand OnExport { get; set; }
    public DigitizedMediaStatusWindowViewModel(ICursorService cursorService,
                                               IWindowService windowService,
                                               IMediaRepository mediaRepository,
                                               IGeometryRepository geometryRepository,
                                               IGebietsassistentRepository gebietsassistentRepository,
                                               IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent)
    {
        _cursorService = cursorService;
        _windowService = windowService;
        _mediaRepository = mediaRepository;
        _geometryRepository = geometryRepository;
        _gebietsassistentRepository = gebietsassistentRepository;
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;

        _digitisationStateProNames = new List<string>()
            {
                "MEDIUM_ID", "MEDIUM_NAME", "PLZ_Scharf", "DIGITALISIERT_VON", "DIGITALISIERT_ZULETZT_AM",
                "EBENEN_DIGITALISIERT_PROZENT", "EBENE_BEZEICHNUNG", "ARBEITSSTATUS"
            };

        OnExport = new RelayCommand(OnFileExport);

    }

    public async void OnWindowLoaded()
    {
        await OnInitialized();
    }

    public void OnWindowClosed()
    {

    }

    public void OnSearchTextChanged()
    {
        if (SearchText.Length > 0)
            MediaDigitizationStates = _mediaDigitizationStateList.FilterGenericListByTextInput(SearchText, _digitisationStateProNames);
        else
            MediaDigitizationStates = _mediaDigitizationStateList;
    }

    public void OnMediaMouseDoubleClick()
    {
        _windowService.ShowWindow<MediaDigitizerWindow>();
    }

    public void OnTourMouseDoubleClick()
    {
        _windowService.ShowWindow<MediaDigitizerWindow>();
    }

    private async Task OnInitialized()
    {
        //SpreadTypes = GetAllSpreadingTypes();
        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        _cursorService.SetCursor(Cursors.Wait);
        await SetMediaDigitizationStates();
        await SetTours();
        await SetChangedMedia();
        FillChangedToursListView();
        SetDigitizationCounter();
        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
        _cursorService.SetCursor(Cursors.Arrow);
    }
    private List<string> GetAllSpreadingTypes()
    {
        return _gebietsassistentRepository.GetDestributionMedia()
                .Select(d => d.SpreadingType)
                .Distinct().ToList();
    }
    private async Task SetMediaDigitizationStates()
    {
        if (!(_mediaDigitizationStateList.Count > 0) && _mediaDigitizationStateList != null)
        {
            _mediaDigitizationStateList = (await SetMediaDigitizationStatesAsync()).ToList();
            await GetZipCodeSharpnessState(_mediaDigitizationStateList);
            MediaDigitizationStates = _mediaDigitizationStateList;
            //var result = MediaDigitizationStates.Where(i => i.PLZ_Scharf == "vollständing");
        }
    }
    private async Task<IEnumerable<MediaDigitizationState>> SetMediaDigitizationStatesAsync()
    {
        return await _mediaRepository.GetStateOfDigitizationAsync();
    }
    private async Task GetZipCodeSharpnessState(List<MediaDigitizationState> states)
    {
        foreach (var state in states)
        {
            string scharpness = await _geometryRepositoryGebietsassistent.GetZipCodeSharpnesStateAsync(state.MEDIUM_ID);
            state.PLZ_Scharf = scharpness;
        }
    }
    private async Task SetTours()
    {
        _updatedAndChangedTours = await _geometryRepositoryGebietsassistent.GetUpdatedAndChangedToursAsync();
    }
    private async Task SetChangedMedia()
    {
        var mediaIds = _updatedAndChangedTours.Select(t => t.MediaId).Distinct().ToList();
        _changedMedia = new List<Media>();
        await Task.Run(() =>
        {
            foreach (var id in mediaIds)
            {
                var media = _mediaRepository.FindContinuedByID(Convert.ToInt32(id));
                if (media.Count() > 0)
                    _changedMedia.Add(media.First());
            }
        });
    }
    private void FillChangedToursListView()
    {
        List<Tour> changedMedia = new List<Tour>();
        foreach (var media in _changedMedia)
        {
            changedMedia.AddRange(_updatedAndChangedTours.Where(t => t.MediaId == media.Id.ToString()).ToList());
        }
        TourList = changedMedia;
    }
    private void SetDigitizationCounter()
    {
        int completedCount = CalculateMediaCountDigitizationCompleted();
        int inProgressCount = CalculateMediaCountDigitizationInProgress();
        int notStartedCount = CalculateMediaCountDigitizationNotStarted();

        FinishedText = completedCount.ToString();
        InProcessText = inProgressCount.ToString();
        NotStartedText = notStartedCount.ToString();
    }
    private int CalculateMediaCountDigitizationCompleted()
    {
        return MediaDigitizationStates.Where(m => m.EBENEN_DIGITALISIERT_PROZENT == 100.00
                                                    || m.PLZ_Scharf == ZipCodeSharpness.vollständig.ToString()).Count();
    }
    private int CalculateMediaCountDigitizationInProgress()
    {
        int inProgress = _mediaDigitizationStates.Where(m => (m.EBENEN_DIGITALISIERT_PROZENT != 100.00
                                                        && m.EBENEN_DIGITALISIERT_PROZENT != 0.00))
                                                      .Count();

        int partSharp = _mediaDigitizationStates.Where(m => m.EBENEN_DIGITALISIERT_PROZENT == 0.00
                                                        && m.PLZ_Scharf == ZipCodeSharpness.teilweise.ToString())
                                                      .Count();
        return inProgress + partSharp;

    }
    private int CalculateMediaCountDigitizationNotStarted()
    {
        return _mediaDigitizationStates.Where(m => m.EBENEN_DIGITALISIERT_PROZENT == 0.00).Count();
    }
    private void OnFileExport()
    {
        var folderPath = FileDialogHelper.GetFilePathFromFolderDialog();
        string filePath = $"{folderPath}\\StatusDititalisierung_{DateTime.Now.ToShortDateString()}_{DateTime.Now.Hour}{DateTime.Now.Minute}.xlsx";
        using (ExcelHelper excelHelper = new SyncfusionExcel())
        {
            excelHelper.CreateWorksheet("Toureninfo", MediaDigitizationStates);
            excelHelper.Save(filePath);
        }
    }

}

