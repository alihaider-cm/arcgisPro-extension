using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.Core.Models.DTO;
using ArcGisPlannerToolbox.WPF.Data;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static ArcGisPlannerToolbox.WPF.Events.PlanAdvertisementAreaWizardEvent;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class ConceptDataViewModel : WizardPageViewModel
{
    #region Fields

    private readonly ICursorService _cursorService;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAdvertisementAreaRepository _advertisementAreaRepository;
    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;
    private readonly IAdvertisementAreaStatisticsRepository _advertisementAreaStatisticsRepository;

    private Customer _selectedCustomer;
    private CustomerBranch _selectedCustomerBranch;
    private List<AdvertisementAreaStatistics> _advertisementAreaStatistics = new();
    private List<PolygonGeometry> _selectedOccupancyUnits = new();
    private string _selectedPlanningLevel = string.Empty;

    #endregion

    #region Properties

    private List<CustomerBranchDTO> _nearestCustomerBranches = new();
    public List<CustomerBranchDTO> NearestCustomerBranches
    {
        get { return _nearestCustomerBranches; }
        set { _nearestCustomerBranches = value; OnPropertyChanged(); }
    }

    private List<CustomerBranchDTO> _selectedNearestCustomerBranches = new();
    public List<CustomerBranchDTO> SelectedNearestCustomerBranches
    {
        get { return _selectedNearestCustomerBranches; }
        set { _selectedNearestCustomerBranches = value; OnPropertyChanged(); }
    }

    private List<string> _geometries = new();
    public List<string> Geometries
    {
        get { return _geometries; }
        set { _geometries = value; OnPropertyChanged(); }
    }

    private List<string> _areaNames = new();
    public List<string> AreaNames
    {
        get { return _areaNames; }
        set { _areaNames = value; OnPropertyChanged(); }
    }

    private List<string> _actionTypes = new();
    public List<string> ActionTypes
    {
        get { return _actionTypes; }
        set { _actionTypes = value; OnPropertyChanged(); }
    }

    private List<string> _actionStates = new();
    public List<string> ActionStates
    {
        get { return _actionStates; }
        set { _actionStates = value; OnPropertyChanged(); }
    }

    private string _selectedGeometry = string.Empty;
    public string SelectedGeometry
    {
        get { return _selectedGeometry; }
        set { _selectedGeometry = value; OnPropertyChanged(); }
    }

    private string _selectedAreaName = string.Empty;
    public string SelectedAreaName
    {
        get { return _selectedAreaName; }
        set { _selectedAreaName = value; OnPropertyChanged(); }
    }

    private string _selectedActionType = string.Empty;
    public string SelectedActionType
    {
        get { return _selectedActionType; }
        set { _selectedActionType = value; OnPropertyChanged(); }
    }

    private string _selectedActionState = string.Empty;
    public string SelectedActionState
    {
        get { return _selectedActionState; }
        set { _selectedActionState = value; OnPropertyChanged(); }
    }

    private string _dateText = string.Empty;
    public string DateText
    {
        get { return _dateText; }
        set { _dateText = value; OnPropertyChanged(); }
    }

    private bool _weekSelection;
    public bool WeekSelection
    {
        get { return _weekSelection; }
        set { _weekSelection = value; OnPropertyChanged(); }
    }

    private bool _visibleInMedia;
    public bool VisibleInMedia
    {
        get { return _visibleInMedia; }
        set { _visibleInMedia = value; OnPropertyChanged(); }
    }

    public ICommand OnSaveCommand { get; set; }

    #endregion

    #region Init

    public ConceptDataViewModel(ICursorService cursorService,
                                ICustomerRepository customerRepository,
                                IAdvertisementAreaRepository advertisementAreaRepository,
                                IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent,
                                IAdvertisementAreaStatisticsRepository advertisementAreaStatisticsRepository)
    {
        _cursorService = cursorService;
        _customerRepository = customerRepository;
        _advertisementAreaRepository = advertisementAreaRepository;
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;
        _advertisementAreaStatisticsRepository = advertisementAreaStatisticsRepository;
        OnSaveCommand = new RelayCommand(OnSave);

        SelectedCustomerChanged.Subscribe(x => _selectedCustomer = x);
        CustomerBranchChanged.Subscribe(x => _selectedCustomerBranch = x);
        SelectedOccupancyUnitsChanged.Subscribe(x => _selectedOccupancyUnits = x);
        PlanningLevelChanged.Subscribe(x => _selectedPlanningLevel = x);
    }

    #endregion

    #region Public Methods

    public void OnLoaded()
    {
        IsLastPage = true;

        SetNearestCustomerBranchData();
        SetAdvertisementAreaStatistics();
        SetDataStoringComboboxData();
    }

    public void OnDateInputFocusLost()
    {
        if (!ValidFromDateFormatApproved(DateText))
        {
            var dialogResult = MessageBox.Show("Die Eingabe des Feldes 'Gültig ab' muss vom Format YYYY-KW sein.",
                    "Format fehlerhaft.", MessageBoxButton.OK, MessageBoxImage.Error);
            //if (dialogResult == MessageBoxResult.OK)
            //{

            //}
        }
    }

    #endregion

    #region Private Methods

    private void SetNearestCustomerBranchData()
    {
        var branches = _customerRepository.GetNearestCustomerBranches(_selectedCustomer.Kunden_ID, _selectedCustomerBranch);
        NearestCustomerBranches = branches.ConvertAll(x => new CustomerBranchDTO(x));
    }
    private void SetDataStoringComboboxData()
    {
        var geometry = _advertisementAreaStatistics.Select(s => s.Basisgeometrie).Distinct().ToList();
        var areaName = _advertisementAreaStatistics.Select(s => s.Gebietsbezeichnung).Distinct().ToList();
        var state = Enum.GetNames(typeof(AdvertisementAreaActionTypes)).ToList();
        var actionType = _advertisementAreaStatisticsRepository.GetActionTypes(_selectedCustomer.Kunden_ID).Select(t => t.ActionTypeName).Distinct().ToList();

        geometry.AddRange(new List<string> { "BBE", "PLZ" });
        geometry.OrderBy(g => g);
        Geometries = geometry;
        AreaNames = areaName;
        ActionTypes = actionType;
        ActionStates = state;
        SelectedGeometry = geometry.First();
        SelectedAreaName = areaName.First();
        SelectedActionType = actionType.First();
        SelectedActionState = state.First();
    }
    private void SetAdvertisementAreaStatistics()
    {
        _advertisementAreaStatistics = _advertisementAreaStatisticsRepository.GetCustomerStatisticsByBranch(_selectedCustomerBranch);
    }
    private async Task OnSave()
    {
        var dialogResult = MessageBox.Show("Bitte bestätigen Sie ihre angegebenen Daten.",
                "Werbegebietsplanung sichern", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (dialogResult == MessageBoxResult.Yes)
        {
            await SaveAdvertisementData();
        }
    }
    private bool ValidFromDateFormatApproved(string input)
    {
        if (input == "")
        {
            return true;
        }
        else
        {
            var inputParts = input.Split('-');
            if (inputParts.Count() == 2 && inputParts[0].Length == 4 && inputParts[1].Length == 2)
            {
                var year = Convert.ToInt32(inputParts[0]);
                var calendarWeek = Convert.ToInt32(inputParts[1]);
                if (year == DateTime.Now.Year
                    || year == DateTime.Now.Year - 1
                    || year == DateTime.Now.Year + 1)
                {
                    if (calendarWeek > 0 && calendarWeek <= 52)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    private async Task SaveAdvertisementData()
    {
        ProApp.Current.MainWindow.Cursor = Cursors.Wait;
        _cursorService.SetCursor(Cursors.Wait);

        DateTime latestVersionOfOccupancyUnits;
        var occupancyUnitIds = _selectedOccupancyUnits.Where(x => x.OccupancyUnit == "BBE").Select(x => x.OccupancyUnitId).ToList();
        latestVersionOfOccupancyUnits = _geometryRepositoryGebietsassistent.GetLatestVersionOfZipCodes();

        var advertisementArea = GetAdvertisementArea(latestVersionOfOccupancyUnits);
        var neighborAdvertisementAreas = GetAdvertisementAreas(SelectedNearestCustomerBranches, latestVersionOfOccupancyUnits);

        var areaExists = _advertisementAreaRepository.CheckIfAdvertisementAreaDataExists(advertisementArea.Gebietsbezeichnung, advertisementArea.Filial_ID_Superoffice);
        bool updateData = false;
        if (areaExists)
        {
            var msg = "Es liegt beriets ein Gebiet mit dieser Gebietsbezeichung für diese Filiale vor.\nWählen Sie 'Ja', um das bestehende Gebiet zu aktualisieren oder 'Nein' für die Sicherung als neuen Eintrag.\nFür die letztere Option ist eine Änderung der Gebietsbezeichnung erforderlich.";
            var dialogResult = MessageBox.Show(msg, "Duplikat entdeckt", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (dialogResult == MessageBoxResult.Yes)
            {
                await Task.Run(() =>
                {
                    updateData = true;
                    if (_selectedPlanningLevel == PlanningLevels.BBE.ToString())
                    {
                        _advertisementAreaRepository.SaveAdvertisementArea(advertisementArea, occupancyUnitIds, neighborAdvertisementAreas, updateData);
                    }
                    else if (_selectedPlanningLevel == PlanningLevels.PLZ.ToString())
                    {
                        var zipCodes = GetSelectedZipCodes();
                        _advertisementAreaRepository.SaveAdvertisementArea(advertisementArea, zipCodes, neighborAdvertisementAreas, updateData);
                    }
                    else
                    {
                        var zipCodes = GetSelectedZipCodes();
                        _advertisementAreaRepository.SaveAdvertisementArea(advertisementArea, zipCodes, occupancyUnitIds, neighborAdvertisementAreas, updateData);
                    }
                });

                MessageBox.Show("Das Werbegebiet wurde erfolgreich gespeichert", "Werbegebeitsspeicherung", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
        _cursorService.SetCursor(Cursors.Arrow);
    }

    private AdvertisementArea GetAdvertisementArea(DateTime version)
    {
        var geometry = SelectedGeometry;
        var areaName = SelectedAreaName;
        var validFromDate = DateText;
        var weekSelection = WeekSelection;
        var visibleInMediamaps = VisibleInMedia;
        var state = SelectedActionState;
        var kampagneType = SelectedActionType;


        return new AdvertisementArea()
        {
            Filial_ID_Superoffice = _selectedCustomerBranch.Filial_ID,
            Werbegebietsstatus = state,
            Gebietsbezeichnung = areaName,
            gültig_ab_Jahr_KW = validFromDate,
            Letzte_Änderung_von = Environment.UserName,
            Letzte_Änderung_am = DateTime.Now,
            Basisgeometrie = geometry,
            Stand = version,
            ISIS_sichtbar = Convert.ToByte(visibleInMediamaps),
            Aktionstyp = kampagneType
        };
    }
    private List<AdvertisementArea> GetAdvertisementAreas(List<CustomerBranchDTO> branches, DateTime version)
    {
        List<AdvertisementArea> advertisementAreas = new List<AdvertisementArea>();
        foreach (var branch in branches)
        {
            var geometry = SelectedGeometry;
            var areaName = SelectedAreaName;
            var validFromDate = DateText;
            var weekSelection = WeekSelection;
            var visibleInMediamaps = VisibleInMedia;
            var state = SelectedActionState;
            var kampagneType = SelectedActionType;

            advertisementAreas.Add(new AdvertisementArea()
            {
                Filial_ID_Superoffice = branch.Filial_ID,
                Werbegebietsstatus = state,
                Gebietsbezeichnung = areaName,
                gültig_ab_Jahr_KW = validFromDate,
                Letzte_Änderung_von = Environment.UserName,
                Letzte_Änderung_am = DateTime.Now,
                Basisgeometrie = geometry,
                Stand = version,
                ISIS_sichtbar = Convert.ToByte(visibleInMediamaps),
                Aktionstyp = kampagneType
            });
        }
        return advertisementAreas;
    }
    private Dictionary<string, int> GetSelectedZipCodes()
    {
        var zipCodes = new Dictionary<string, int>();
        foreach (var occupancyUnit in _selectedOccupancyUnits.Where(x => x.OccupancyUnit == "PLZ"))
        {
            var splitResults = occupancyUnit.TourName.Split(' ');
            var zipCode = splitResults.Length > 1 ? splitResults[1] : string.Empty;
            if (!zipCodes.ContainsKey(zipCode))
                zipCodes.TryAdd(zipCode, occupancyUnit.MediaId);
        }
        return zipCodes;
    }

    #endregion

}
