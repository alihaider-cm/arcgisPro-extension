using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping.Events;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.Core.Models.DTO;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class AreaDigitizerWindowViewModel : BindableBase
{
    #region Fields

    private readonly SubscriptionToken _areaSelectionSubscriptionToken;
    private readonly SubscriptionToken _mapSelectionSubscriptionToken;

    private readonly IMapManager _mapManager;
    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;

    private List<OccupancyUnit> _selectedItems = new();
    private List<OccupancyUnitDTO> _allOccupanyUnits = new();
    private bool _foreignMicroZipCodesAccepted = false;

    #endregion

    #region Properties

    private List<OccupancyUnitDTO> _occupancyUnits = new();
    public List<OccupancyUnitDTO> OccupancyUnits
    {
        get { return _occupancyUnits; }
        set { _occupancyUnits = value; OnPropertyChanged(); }
    }

    private OccupancyUnitDTO _selectedOccupancyUnit = new();
    public OccupancyUnitDTO SelectedOccupancyUnit
    {
        get { return _selectedOccupancyUnit; }
        set { _selectedOccupancyUnit = value; OnPropertyChanged(); }
    }

    private bool _autoSelectInMap = true;
    public bool AutoSelectInMap
    {
        get { return _autoSelectInMap; }
        set 
        { 
            _autoSelectInMap = value; 
            OnPropertyChanged();
            // Fire off async task when property changes
            _ = UpdateMapSelectionOnAutoSelectChanged();
        }
    }

    private int _grossHouseholdSum;
    public int GrossHouseholdSum
    {
        get { return _grossHouseholdSum; }
        set { _grossHouseholdSum = value; OnPropertyChanged(); }
    }

    private int _netHouseholdSum;
    public int NetHouseholdSum
    {
        get { return _netHouseholdSum; }
        set { _netHouseholdSum = value; OnPropertyChanged(); }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get { return _searchText; }
        set { _searchText = value; OnPropertyChanged(); }
    }

    public ICommand UpdateConnectionTableCommand { get; set; }
    public ICommand CheckedAllCommand { get; set; }
    public ICommand UncheckedAllCommand { get; set; }
    public ICommand SaveCommand { get; set; }

    #endregion

    #region Initialization

    public AreaDigitizerWindowViewModel(IMapManager mapManager, IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent)
    {
        _mapManager = mapManager;
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;

        UpdateConnectionTableCommand = new RelayCommand(() => DistributionSelectionTableChangedEvent.Publish(1));
        CheckedAllCommand = new RelayCommand(OnCheckedAllOccupanyUnits);
        UncheckedAllCommand = new RelayCommand(OnUncheckedAllOccupancyUnits);
        SaveCommand = new RelayCommand(OnSave);
        _areaSelectionSubscriptionToken = AreaSelectionChangedEvent.Subscribe(async (x) => await UpdateMapAndOccupancyUnits(x));
        _mapSelectionSubscriptionToken = MapSelectionChangedEvent.Subscribe(async x => await OnMapSelectionChanged(x));
    }

    #endregion

    #region Public Methods

    public void OnWindowClosed()
    {
        AreaSelectionChangedEvent.Unsubscribe(_areaSelectionSubscriptionToken);
        MapSelectionChangedEvent.Unsubscribe(_mapSelectionSubscriptionToken);
    }

    public async Task OnZipCodeSelectionChanged(bool isChecked, string microZipCode)
    {
        SelectedOccupancyUnit = OccupancyUnits.First(x => x.MicroZipCode == microZipCode);
        //if (SelectedOccupancyUnit is not null)
        //{
        SelectedOccupancyUnit.IsChecked = isChecked;
        await UpdateCurrentSelection(SelectedOccupancyUnit);
        //}
    }

    public async Task OnSelectedItemChanged(OccupancyUnitDTO occupancyUnit)
    {
        SelectedOccupancyUnit = occupancyUnit;
        OccupancyUnits.First(x => x.MicroZipCode == SelectedOccupancyUnit.MicroZipCode).IsChecked = SelectedOccupancyUnit.IsChecked;
        await UpdateCurrentSelection(SelectedOccupancyUnit);
    }

    public void OnSearchTextChanged()
    {
        if (SearchText.Length > 0)
            OccupancyUnits = _allOccupanyUnits.Where
                    (o => o.Districts == null || o.Districts.ToLower().Contains(SearchText.ToLower())
                    || (o.ZipCodeName == null || o.ZipCodeName.ToLower().Contains(SearchText.ToLower())))
                    .ToList();
        else
            OccupancyUnits = _allOccupanyUnits;

    }

    #endregion

    #region Private Methods

    private void DeleteCurrentSelection(OccupancyUnit selectedOccupancyUnit) => _selectedItems.Remove(selectedOccupancyUnit);
    private void SaveCurrentSelection(OccupancyUnit selectedOccupancyUnit) => _selectedItems.Add(selectedOccupancyUnit);
    //private void OnAreaSelectionChanged(List<OccupancyUnit> occupancyUnits) => UpdateOccupancyUnits(occupancyUnits);
    //private void UpdateOccupancyUnits(List<OccupancyUnit> occupancyUnits) => UpdateOccupancyUnits(occupancyUnits.ConvertAll<OccupancyUnitDTO>(x => new(x)));

    private async Task UpdateMapAndOccupancyUnits(List<OccupancyUnitDTO> occupancyUnits)
    {
        UpdateOccupancyUnits(occupancyUnits);
        _selectedItems.Clear();
        
        var checkedUnits = OccupancyUnits.Where(x => x.IsChecked).ToList();
        
        // Update selection and sums immediately
        foreach (var unit in checkedUnits)
        {
            SelectedOccupancyUnit = new(unit);
            SaveCurrentSelection(unit);
        }

        // Update sums immediately
        GrossHouseholdSum = _selectedItems.Select(x => x.GrossHousehold).Sum();
        NetHouseholdSum = _selectedItems.Select(x => x.NetHouesehold).Sum();

        // Handle map selection in background if needed
        if (AutoSelectInMap && checkedUnits.Any())
        {
            // Use batch selection for better performance
            _ = _mapManager.SelectManyFeaturesOnMicroZipCodeLayer(
                checkedUnits.Select(x => x.MicroZipCode).ToList());
        }
    }
    private void UpdateOccupancyUnits(List<OccupancyUnitDTO> occupancyUnits)
    {
        OccupancyUnits = new(occupancyUnits);

        _allOccupanyUnits.Clear();
        _allOccupanyUnits.AddRange(occupancyUnits);
    }
    private async Task UpdateMapAsync(bool isChecked)
    {
        if (isChecked)
            await _mapManager.SelectFeatureOnMicroZipCodeLayer(SelectedOccupancyUnit.MicroZipCode);
        else
            await _mapManager.DeselectFeatureOnMicroZipCodeLayer(SelectedOccupancyUnit.MicroZipCode);
    }
    private async Task OnMapSelectionChanged(MapSelectionChangedEventArgs args)
    {
        if (args.IsPointSelection)
        {
            _foreignMicroZipCodesAccepted = false;
            var selectedFIDList = args.Selection.ToDictionary().SelectMany(x => x.Value).ToList();
            var selectedMicroZipCodes = await _mapManager.GetSelectedMicroZipCodeFromLayer(selectedFIDList);
            await ExtractAndAddForeignMicroZipCodes(selectedMicroZipCodes);
            await UpdateCheckedOccupancyUnits(selectedMicroZipCodes);
        }
    }
    private async Task UpdateCurrentSelection(OccupancyUnitDTO occupancyUnit)
    {
        // Start map update in background if needed
        Task? mapUpdateTask = null;
        if (AutoSelectInMap)
        {
            mapUpdateTask = UpdateMapAsync(occupancyUnit.IsChecked);
        }

        // Update selection and sums immediately
        if (occupancyUnit.IsChecked)
            SaveCurrentSelection(occupancyUnit);
        else
            DeleteCurrentSelection(occupancyUnit);

        GrossHouseholdSum = _selectedItems.Select(x => x.GrossHousehold).Sum();
        NetHouseholdSum = _selectedItems.Select(x => x.NetHouesehold).Sum();

        // Wait for map update to complete in background if it was started
        if (mapUpdateTask != null)
        {
            await mapUpdateTask;
        }
    }
    private async Task ExtractAndAddForeignMicroZipCodes(List<string> microZipCodesFromMap)
    {
        var existingZipCodes = OccupancyUnits.Select(x => x.ZipCode);
        var microZipCodesToDeselect = new List<string>();

        foreach (var microZipCode in microZipCodesFromMap)
        {
            var foreignZipCode = microZipCode.Substring(0, 5);
            if (!existingZipCodes.Contains(foreignZipCode))
            {
                if (!_foreignMicroZipCodesAccepted)
                {
                    var result = MessageBox.Show("Diese mPLZ befindet sich in einer fremden PLZ. Trotzdem fortfahren?", 
                        "Digitalisierung außerhalb der Touren-PLZ",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        
                    if (result == MessageBoxResult.Yes)
                    {
                        _foreignMicroZipCodesAccepted = true;
                        var foreignUnits = _geometryRepositoryGebietsassistent.GetOccupancyUnits(foreignZipCode);
                        var foreignUnitsDTO = foreignUnits.ConvertAll<OccupancyUnitDTO>(x => new(x));
                        OccupancyUnits.ForEach(x => foreignUnitsDTO.Add(x));
                        UpdateOccupancyUnits(foreignUnitsDTO);
                    }
                    else
                    {
                        // Collect all foreign microZipCodes to deselect
                        microZipCodesToDeselect.Add(microZipCode);
                        continue;
                    }
                }
                else
                {
                    var foreignUnits = _geometryRepositoryGebietsassistent.GetOccupancyUnits(foreignZipCode);
                    var foreignUnitsDTO = foreignUnits.ConvertAll<OccupancyUnitDTO>(x => new(x));
                    OccupancyUnits.ForEach(x => foreignUnitsDTO.Add(x));
                    UpdateOccupancyUnits(foreignUnitsDTO);
                }
            }
        }

        // Deselect rejected foreign microZipCodes from map
        if (microZipCodesToDeselect.Any())
        {
            _ = _mapManager.DeselectManyFeaturesOnMicroZipCodeLayer(microZipCodesToDeselect);
            
            // Remove rejected microZipCodes from the original list
            microZipCodesFromMap.RemoveAll(x => microZipCodesToDeselect.Contains(x));
        }
    }
    private async Task UpdateCheckedOccupancyUnits(List<string> microZipCodesFromMap)
    {
        // Clear current selection before updating
        _selectedItems.Clear();

        // Create lists for selection and deselection
        var toSelect = new List<string>();
        var toDeselect = new List<string>();

        foreach (var unit in OccupancyUnits)
        {
            if (microZipCodesFromMap.Contains(unit.MicroZipCode))
            {
                unit.IsChecked = true;
                SaveCurrentSelection(unit);
                toSelect.Add(unit.MicroZipCode);
            }
            else
            {
                unit.IsChecked = false;
                toDeselect.Add(unit.MicroZipCode);
            }
        }

        // Update sums after all selections are processed
        GrossHouseholdSum = _selectedItems.Select(x => x.GrossHousehold).Sum();
        NetHouseholdSum = _selectedItems.Select(x => x.NetHouesehold).Sum();

        // Update map in background if needed
        if (AutoSelectInMap)
        {
            if (toSelect.Any())
            {
                _ = _mapManager.SelectManyFeaturesOnMicroZipCodeLayer(toSelect);
            }
            if (toDeselect.Any())
            {
                _ = _mapManager.DeselectManyFeaturesOnMicroZipCodeLayer(toDeselect);
            }
        }
    }
    private async Task OnUncheckedAllOccupancyUnits()
    {
        //foreach(var occupancyUnit in OccupancyUnits)
        //{
        //    occupancyUnit.IsChecked = false;
        //    await _mapManager.DeselectFeatureOnMicroZipCodeLayer(occupancyUnit.MicroZipCode);
        //}
        OccupancyUnits.ForEach(x => x.IsChecked = false);
        await _mapManager.DeselectManyFeaturesOnMicroZipCodeLayer(OccupancyUnits.Select(x => x.MicroZipCode).ToList());
    }
    private async Task OnCheckedAllOccupanyUnits()
    {
        //foreach (var occupancyUnit in OccupancyUnits)
        //{
        //    occupancyUnit.IsChecked = true;
        //    await _mapManager.SelectFeatureOnMicroZipCodeLayer(occupancyUnit.MicroZipCode);
        //}
        OccupancyUnits.ForEach(x => x.IsChecked = true);
        await _mapManager.SelectManyFeaturesOnMicroZipCodeLayer(OccupancyUnits.Select(x => x.MicroZipCode).ToList());
    }
    private void OnSave()
    {
        var newUnits = OccupancyUnits.Where(x => x.IsChecked).ToList();
        //var oldUnits = _allOccupanyUnits.Where(y => y.IsChecked).ToList();
        //var selectedOccupancyUnits = OccupancyUnits.Where(x => x.IsChecked).Except(_allOccupanyUnits.Where(y => y.IsChecked)).ToList();
        var currentTime = DateTime.Now;
        var username = Environment.UserName;
        MessageBox.Show($"{newUnits.Count} Elemente werden übernommen. Übernahme bestätigen.");
        newUnits.ForEach(x =>
        {
            x.LastModified = currentTime;
            x.Author = username;
        });
        AreasPublishEvent.Publish(newUnits);
    }

    private async Task UpdateMapSelectionOnAutoSelectChanged()
    {
        if (AutoSelectInMap)
        {
            // Select all checked items in map
            var checkedMicroZipCodes = OccupancyUnits.Where(x => x.IsChecked)
                                                    .Select(x => x.MicroZipCode)
                                                    .ToList();
            if (checkedMicroZipCodes.Any())
            {
                await _mapManager.SelectManyFeaturesOnMicroZipCodeLayer(checkedMicroZipCodes);
            }
        }
        else
        {
            // Deselect all items in map
            var allMicroZipCodes = OccupancyUnits.Select(x => x.MicroZipCode).ToList();
            if (allMicroZipCodes.Any())
            {
                await _mapManager.DeselectManyFeaturesOnMicroZipCodeLayer(allMicroZipCodes);
            }
        }
    }

    #endregion

}
