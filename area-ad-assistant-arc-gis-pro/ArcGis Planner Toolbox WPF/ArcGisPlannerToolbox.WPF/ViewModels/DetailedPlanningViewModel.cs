using ArcGIS.Core.Events;
using ArcGIS.Desktop.Mapping.Events;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.Core.Models.DTO;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Helpers;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.Views;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class DetailedPlanningViewModel : WizardPageViewModel
{
    #region Private Fields

    private readonly SubscriptionToken CustomerChangedSubscriptionToken;
    private readonly SubscriptionToken AnalysisIdSubscriptionToken;
    private readonly SubscriptionToken BranchCreatedSubscriptionToken;
    private readonly SubscriptionToken PlanningNumberChangedSubscriptionToken;
    private readonly SubscriptionToken _mapSelectionSubscriptionToken;
    private readonly SubscriptionToken _securePlanningSelectionChangedSubscriptionToken;
    private readonly IPlanningRepository _planningRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapManager _mapManager;
    private readonly IWindowService _windowService;

    private int _currentAnalysisId, _currentPlanningNumber, _currentConceptNumber;
    private List<PlanningData> _planningDataList = new();
    private List<string> _preDefinedBranchNumbers = new();
    private List<string> _selectedFeatures = new();
    private List<string> _geoKeySnapshot = new();
    private List<PlanningDataArea> _currentAreasOfSelectedBranch = new();
    public PlanningData _securedPlanningSelectedItem = null;

    #endregion

    #region Properties

    private List<CustomerBranchDTO> _plannedBranches = new();
    public List<CustomerBranchDTO> PlannedBranches
    {
        get { return _plannedBranches; }
        set { _plannedBranches = value; OnPropertyChanged(); }
    }

    private CustomerBranchDTO _selectedPlannedBranch = new();
    public CustomerBranchDTO SelectedPlannedBranch
    {
        get { return _selectedPlannedBranch; }
        set { _selectedPlannedBranch = value; OnPropertyChanged(); }
    }

    private List<PlanningData> _planningData = new();
    public List<PlanningData> PlanningData { get { return _planningData; } set { _planningData = value; OnPropertyChanged(); } }

    private List<PlanningDataAreaDTO> _currentAreasOfInterest = new();
    public List<PlanningDataAreaDTO> CurrentAreasOfInterest
    {
        get { return _currentAreasOfInterest; }
        set { _currentAreasOfInterest = value; OnPropertyChanged(); }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get { return _searchText; }
        set { _searchText = value; OnPropertyChanged(); }
    }
    private bool _saveChangesEnable = false;
    public bool SaveChangesButtonEnable
    {
        get { return _saveChangesEnable; }
        set { _saveChangesEnable = value; OnPropertyChanged(); }
    }

    public Customer SelectedCustomer { get; set; }
    public ICommand ListViewCheckBoxCheckedCommand { get; set; }
    public ICommand ListViewCheckBoxUncheckedCommand { get; set; }
    public ICommand AreaListViewCheckBoxCheckedCommand { get; set; }
    public ICommand AreaListViewCheckBoxUncheckedCommand { get; set; }
    public ICommand SetInitiallyCommand { get; set; }
    public ICommand AddBranchCommand { get; set; }
    public ICommand SaveChangesCommand { get; set; }
    public ICommand ClearChangesCommand { get; set; }

    #endregion

    #region Initialization

    public DetailedPlanningViewModel(IPlanningRepository planningRepository, ICustomerRepository customerRepository, IMapManager mapManager, IWindowService windowService)
    {
        _mapManager = mapManager;
        _windowService = windowService;
        _planningRepository = planningRepository;
        _customerRepository = customerRepository;

        ListViewCheckBoxCheckedCommand = new RelayCommand<string>(OnCheckBoxChecked);
        ListViewCheckBoxUncheckedCommand = new RelayCommand<string>(OnCheckBoxUnchecked);
        AreaListViewCheckBoxCheckedCommand = new RelayCommand<string>(async (x) => await AreaListOnCheckBoxChecked(x));
        AreaListViewCheckBoxUncheckedCommand = new RelayCommand<string>(async (x) => await AreaListOnCheckBoxUnchecked(x));
        SetInitiallyCommand = new RelayCommand(SetInitially);
        AddBranchCommand = new RelayCommand(ShowAddBranchWindow);
        ClearChangesCommand = new ArcGIS.Desktop.Framework.RelayCommand(OnClearChanges);
        SaveChangesCommand = new ArcGIS.Desktop.Framework.RelayCommand(OnSaveChanges);
        CustomerChangedSubscriptionToken = MultiBranchWizardSteps.CustomerChanged.Subscribe(x => SelectedCustomer = x);
        AnalysisIdSubscriptionToken = MultiBranchWizardSteps.AnalysisIdChanged.Subscribe(x => _currentAnalysisId = x);
        BranchCreatedSubscriptionToken = BranchCreatedEvent.Subscribe(OnBranchAdded);
        PlanningNumberChangedSubscriptionToken = PlanningNumberChangedEvent.Subscribe(x => _currentPlanningNumber = x);
        _mapSelectionSubscriptionToken = MapSelectionChangedEvent.Subscribe(async x => await OnMapSelectionChanged(x));
        _securePlanningSelectionChangedSubscriptionToken = MultiBranchWizardSteps.SecuredPlanningSelectionChanged.Subscribe(x => _securedPlanningSelectedItem = x);
    }

    #endregion

    #region Public Methods

    public void OnLoaded()
    {
        Heading = "Feinplanung";
        NextStep = 10;
        AllowNext = true;
        PopulateBranchData();
    }
    public async void OnPlannedBranchesSelectionChanged()
    {
        _selectedFeatures.Clear();
        SaveChangesButtonEnable = false;
        if (SelectedPlannedBranch is not null)
            await LoadAndViewPlanningDataAndAreas();
    }
    public void OnCheckBoxChecked(string branchNumber) => _preDefinedBranchNumbers.Add(branchNumber);
    public void OnCheckBoxUnchecked(string branchNumber) => _preDefinedBranchNumbers.Remove(branchNumber);
    public async Task AreaListOnCheckBoxChecked(string geoKey)
    {
        if (!_geoKeySnapshot.Contains(geoKey))
            _selectedFeatures.Add(geoKey);
        else
            _selectedFeatures.Remove(geoKey); // remove from list since user reverted the changes

        TryEnableSubmitChangesButton();
        await _mapManager.SelectFeatureOnZipCodeLayer(geoKey);
    }
    public async Task AreaListOnCheckBoxUnchecked(string geoKey)
    {
        if (_geoKeySnapshot.Contains(geoKey)) // already checked
        {
            if (!_selectedFeatures.Contains(geoKey))
                _selectedFeatures.Add(geoKey); // detect changes
        }
        else
            _selectedFeatures.Remove(geoKey);

        TryEnableSubmitChangesButton();
        await _mapManager.DeselectFeatureOnZipCodeLayer(geoKey);
    }
    public void OnTextChanged()
    {
        if (SearchText.Length > 0)
        {
            var filterColumns = new List<string>() { "Filialname", "Filial_Nr", "ORT", "Straße", "PLZ" };
            var branches = PlannedBranches.FilterGenericListByTextInput(SearchText, filterColumns);
            PlannedBranches = null;
            PlannedBranches = branches;
        }
        else
        {
            PlannedBranches = null;
            PlannedBranches = GetCustomerBranchInfo(_planningDataList);
        }
    }
    public void SetInitially()
    {
        _planningRepository.SavePredefinedBranches(_preDefinedBranchNumbers, SelectedCustomer.Kunden_ID);
        PlannedBranches.ForEach(x => x.IsChecked = false);
    }

    #endregion

    #region Private Methods

    private void PopulateBranchData()
    {
        _planningDataList = GetPlanningData();
        if (PlannedBranches.Count > 0)
            PlannedBranches = null;

        PlannedBranches = GetCustomerBranchInfo(_planningDataList);
    }
    private List<PlanningData> GetPlanningData() => _planningRepository.GetPlanningData();
    private List<CustomerBranchDTO> GetCustomerBranchInfo(List<PlanningData> planningData)
    {
        var customerBranches = planningData.Select(p => p.Filial_Nr).ToList();
        var branches = _customerRepository.GetBranchesByBranchNumbers(SelectedCustomer.Kunden_ID, customerBranches);
        SetBranchPlannedByAlgorithmState(planningData, branches);
        return branches.ConvertAll<CustomerBranchDTO>(x => new(x));
    }
    private void SetBranchPlannedByAlgorithmState(List<PlanningData> planningData, List<CustomerBranch> branches)
    {
        foreach (var branch in branches)
        {
            foreach (var item in planningData.Where(x => x.Filial_Nr == branch.Filial_Nr))
            {
                if (item.Auflage > 0)
                    branch.VonAlgorithmusGeplant = true;
                else
                    branch.VonAlgorithmusGeplant = false;
            }
        }
    }
    private async Task LoadAndViewPlanningDataAndAreas()
    {
        ClearZipCodeLayer();
        PlanningData = _planningDataList.Where(x => x.Filial_Nr == SelectedPlannedBranch.Filial_Nr).ToList();
        _planningRepository.ExecuteStoredProcedurePlanningSelectionTable(SelectedPlannedBranch.Filial_Nr, _currentAnalysisId, SelectedCustomer.Kunden_ID);
        CurrentAreasOfInterest = GetAreasOfInterest();
        _currentAreasOfSelectedBranch = GetPlanningDataAreas(SelectedPlannedBranch.Filial_Nr);
        //_selectedFeatures = _currentAreasOfSelectedBranch.Select(a => a.Geokey).ToList();
        CheckPlanningDataAreas(_currentAreasOfSelectedBranch, CurrentAreasOfInterest);
        //var geokeys = _currentAreasOfSelectedBranch.Select(a => a.Geokey).ToList();
        var overlappingAreas = _planningRepository.GetOverlappingPlanningDataByBranchNumber(SelectedPlannedBranch.Filial_Nr, _currentAnalysisId);
        await CreateMultiBranchLayer(overlappingAreas, SelectedPlannedBranch.Filial_Nr);
        _geoKeySnapshot = CurrentAreasOfInterest.Where(x => x.IsChecked).Select(x => x.Geokey).ToList();
        foreach (var geoKey in _geoKeySnapshot)
            await _mapManager.SelectFeatureOnZipCodeLayer(geoKey);
    }
    private void ClearZipCodeLayer()
    {
        _mapManager.ClearCurrentZipCodeLayerSelection();
        _planningRepository.ExecuteStoredProcedurePlanningSelectionTable("", 0, 0);
    }
    private List<PlanningDataAreaDTO> GetAreasOfInterest() => _planningRepository.GetGeokysFromTempZipCodeTable().ConvertAll<PlanningDataAreaDTO>(x => new(x));
    private List<PlanningDataArea> GetPlanningDataAreas(string branchNumber) => _planningRepository.GetPlanningDataAreasByBranchNumber(branchNumber);
    private void CheckPlanningDataAreas(List<PlanningDataArea> areasToBeChecked, List<PlanningDataAreaDTO> areasOfInterest)
    {
        foreach (var areaOfI in areasOfInterest)
            foreach (var area in areasToBeChecked)
                if (areaOfI.Geokey == area.Geokey)
                    areaOfI.IsChecked = true;
    }
    private async Task CreateMultiBranchLayer(List<PlanningDataArea> areas, string branchNumber)
    {
        var planningNumber = areas.Select(a => a.Planungs_Nr).FirstOrDefault();
        await _mapManager.CreateMultiBranchPlanningLayer(areas, SelectedCustomer.Kunde, planningNumber.ToString(), branchNumber);
    }
    private void ShowAddBranchWindow()
    {
        _windowService.ShowWindow<AddBranchWindow>();
    }
    private async void OnBranchAdded(CustomerBranch branch)
    {
        _planningRepository.DeleteBranchFromFormerBranchSelection(SelectedCustomer.Kunden_ID, branch.Filial_Nr);
        await _planningRepository.ExecuteUploadBranchDataToDatabase(SelectedCustomer.Kunden_ID, branch);
        _planningRepository.ExecuteAutoPlannerForNewBranchesStoredProcedure(_currentAnalysisId, "PLZ", _currentPlanningNumber);
        PopulateBranchData();
    }
    private async Task OnClearChanges()
    {
        // TODO: verify behavior
        var geoKeys = CurrentAreasOfInterest.Where(x => x.IsChecked).Select(x => x.Geokey).ToList();
        if (geoKeys.Count > 0)
            await DeselectGeometriesOnMap(geoKeys);

        CurrentAreasOfInterest.ForEach(x => x.IsChecked = false);
        CheckPlanningDataAreas(_currentAreasOfSelectedBranch, CurrentAreasOfInterest);
        await SelectGeometriesOnMap(_geoKeySnapshot);
        _selectedFeatures.Clear();
        TryEnableSubmitChangesButton();
        await _mapManager.ZoomToMultiBranchLayer();
    }
    private async Task SelectGeometriesOnMap(List<string> geokeys)
    {
        await _mapManager.SelectManyFeaturesOnZipCodeLayer("PLZ_dynamisch.PLZ", geokeys);
        await _mapManager.ZoomToZipCodeLayer();
    }
    private async Task DeselectGeometriesOnMap(List<string> geokeys) => await _mapManager.DeselectManyFeaturesOnZipCodeLayer("PLZ_dynamisch.PLZ", geokeys);
    private async Task OnMapSelectionChanged(MapSelectionChangedEventArgs args)
    {
        if (args.IsPointSelection)
        {
            var selectedFIDList = args.Selection.ToDictionary().SelectMany(x => x.Value).ToList();
            var selectedZipCodes = await _mapManager.GetSelectedZipCodeFromLayer(selectedFIDList);
            UpdateListSelection(selectedZipCodes);
            TryEnableSubmitChangesButton();
        }
    }
    private void UpdateListSelection(List<string> zipCodes)
    {
        CurrentAreasOfInterest.ForEach(x => x.IsChecked = false);
        zipCodes.ForEach(x =>
        {
            var selectedArea = CurrentAreasOfInterest.Where(area => area.Geokey.Equals(x)).FirstOrDefault();
            if (selectedArea is not null)
                selectedArea.IsChecked = true;
        });
    }
    private async Task OnSaveChanges()
    {
        var selectedFeaturesByAutoplanner = _currentAreasOfSelectedBranch.Select(a => a.Geokey).ToList();
        var foreignGeokeys = GetDifference(selectedFeaturesByAutoplanner, _selectedFeatures).ToList();
        var mergedFeatures = new List<string>();
        var deselectedGeokeys = new List<string>();
        //var deselectedGeokeys = GetDifference(_selectedFeatures, selectedFeaturesByAutoplanner).ToList();
        _selectedFeatures.ForEach(x =>
        {
            if (selectedFeaturesByAutoplanner.Contains(x))
                deselectedGeokeys.Add(x);
        });
        for (int i = 0; i < deselectedGeokeys.Count; i++)
        {
            var geoKey = deselectedGeokeys[i];
            if (!_selectedFeatures.Contains(geoKey))
                deselectedGeokeys.Remove(geoKey);
        }

        var currentOwners = GetCurrentOwnersOfGeokeys(foreignGeokeys);
        var unplannedGeokeys = GetDifference(currentOwners.Select(o => o.Geokey).ToList(), foreignGeokeys).ToList();
        var planningChangesString = GetPlanningChangesString(currentOwners, deselectedGeokeys, unplannedGeokeys);
        var dialogResult = MessageBox.Show($"Möchten Sie wirklich folgende Änderungen durchführen: {planningChangesString}",
                "Planungsänderungen", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (dialogResult == MessageBoxResult.Yes)
            await UpdateChanges(currentOwners, deselectedGeokeys, unplannedGeokeys);
    }
    private void TryEnableSubmitChangesButton()
    {
        SaveChangesButtonEnable = AutoplannerResultsDifferFromCurrentSelection();
    }
    private bool AutoplannerResultsDifferFromCurrentSelection()
    {
        if (_selectedFeatures.Count > 0)
        {
            foreach (var feature in _selectedFeatures)
            {
                if (_geoKeySnapshot.Contains(feature))
                    return true;

                return true;
            }
        }
        return false; // No Changes Pending
    }
    private List<T> GetDifference<T>(List<T> baseList, List<T> comparisonList)
    {
        var differenceList = new List<T>();
        foreach (var item in comparisonList)
        {
            if (!baseList.Contains(item))
                differenceList.Add(item);
        }
        return differenceList;
    }
    private List<PlanningDataArea> GetCurrentOwnersOfGeokeys(List<string> geokeys) => _planningRepository.GetCurrentOwnersOfGeokeys(geokeys);
    private string GetPlanningChangesString(List<PlanningDataArea> currentOwners, List<string> deselectedGeokeys, List<string> unplannedGeokeys)
    {
        string changes = "";
        foreach (var owner in currentOwners)
        {
            changes += Environment.NewLine;
            changes += $">> Geokey: {owner.Geokey} | Filiale vorher: {owner.Filial_Nr} --> Filiale aktuell: {SelectedPlannedBranch.Filial_Nr}";
        }
        foreach (var geokey in deselectedGeokeys)
        {
            changes += Environment.NewLine;
            changes += $">> Geokey: {geokey} | aktuell geplant--> entfernen";
        }
        foreach (var geokey in unplannedGeokeys)
        {
            changes += Environment.NewLine;
            changes += $">> Geokey: {geokey} | aktuell ungeplant --> hinzufügen";
        }
        return changes;
    }
    private async Task UpdateChanges(List<PlanningDataArea> currentOwners, List<string> deselectedGeokeys, List<string> unplannedGeokeys)
    {
        try
        {
            _planningRepository.MoveAreaToDifferentBranch(currentOwners, SelectedPlannedBranch.Filial_Nr);
            _planningRepository.RemoveSelectedAreaFromBranch(deselectedGeokeys, SelectedPlannedBranch.Filial_Nr);
            _planningRepository.AddUnselectedAreaToBranch(unplannedGeokeys, SelectedPlannedBranch.Filial_Nr);
            _selectedFeatures.Clear();
            TryEnableSubmitChangesButton();
            SetPlanningData();
            await LoadAndViewPlanningDataAndAreas();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void SetPlanningData()
    {
        if (_securedPlanningSelectedItem is not null)
        {
            _planningDataList = _planningRepository.GetPlanningDataByPlanningNumberAndConceptNumber(_securedPlanningSelectedItem.Planungs_Nr, _securedPlanningSelectedItem.Konzept_Nr);
            _currentPlanningNumber = _securedPlanningSelectedItem.Planungs_Nr;
            _currentConceptNumber = _securedPlanningSelectedItem.Konzept_Nr; // not using 
            _currentAnalysisId = _securedPlanningSelectedItem.Analyse_Id;
            _planningRepository.FormerPlanningNumber = _securedPlanningSelectedItem.Planungs_Nr;
        }
    }

    #endregion

}
