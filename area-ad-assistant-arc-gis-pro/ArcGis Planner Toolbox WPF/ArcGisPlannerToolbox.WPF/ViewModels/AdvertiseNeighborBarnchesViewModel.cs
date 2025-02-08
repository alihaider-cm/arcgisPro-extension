using ArcGIS.Core.Events;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using static ArcGisPlannerToolbox.WPF.Events.PlanAdvertisementAreaWizardEvent;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class AdvertiseNeighborBarnchesViewModel : WizardPageViewModel
{
    #region Fields

    private readonly IMapManager _mapManager;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAdvertisementAreaRepository _advertisementAreaRepository;
    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;
    private readonly IAdvertisementAreaGeometryRepository _advertisementAreaGeometryRepository;
    private readonly IAdvertisementAreaStatisticsRepository _advertisementAreaStatisticsRepository;

    private readonly SubscriptionToken _branchChangedToken;
    private CustomerBranch _selectedBranch = new();
    private List<CustomerBranch> _customerBranchesCollection = new();
    private List<AdvertisementAreaStatistics> _advertisementAreaStatisticsNearestCustomerBranches = new();
    private string _currentNeigborRadioButtonText = "Alle";

    #endregion

    #region Properties

    private List<CustomerBranch> _customerBranches = new();
    public List<CustomerBranch> CustomerBranches
    {
        get { return _customerBranches; }
        set { _customerBranches = value; OnPropertyChanged(); }
    }

    private List<AdvertisementAreaStatistics> _advertisementAreaStatistics = new();
    public List<AdvertisementAreaStatistics> AdvertisementAreaStatistics
    {
        get { return _advertisementAreaStatistics; }
        set { _advertisementAreaStatistics = value; OnPropertyChanged(); }
    }

    private AdvertisementAreaStatistics _selectedAdvertisementAreaStatistics = new();
    public AdvertisementAreaStatistics SelectedAdvertisementAreaStatistics
    {
        get { return _selectedAdvertisementAreaStatistics; }
        set { _selectedAdvertisementAreaStatistics = value; OnPropertyChanged(); }
    }

    private CustomerBranch _selectedCustomerBranch = new();
    public CustomerBranch SelectedCustomerBranch
    {
        get { return _selectedCustomerBranch; }
        set { _selectedCustomerBranch = value; OnPropertyChanged(); }
    }

    public ICommand AreaChangedCommand { get; set; }


    #endregion

    #region Initialization

    public AdvertiseNeighborBarnchesViewModel(
        IMapManager mapManager,
        ICustomerRepository customerRepository,
        IAdvertisementAreaStatisticsRepository advertisementAreaStatisticsRepository,
        IAdvertisementAreaRepository advertisementAreaRepository,
        IAdvertisementAreaGeometryRepository advertisementAreaGeometryRepository,
        IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent)
    {
        _mapManager = mapManager;
        _advertisementAreaRepository = advertisementAreaRepository;
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;
        _advertisementAreaGeometryRepository = advertisementAreaGeometryRepository;
        _customerRepository = customerRepository;
        _advertisementAreaStatisticsRepository = advertisementAreaStatisticsRepository;

        AreaChangedCommand = new RelayCommand<string>(OnAreaChanged);
        _branchChangedToken = CustomerBranchChanged.Subscribe(x => _selectedBranch = x);
    }

    #endregion

    #region Public Methods

    public void OnLoaded()
    {
        NextStep = 4;
        Heading = "Werbegebiete von Nachbarfilialen";
        AllowNext = true;
        SetNearestCustomerBranchData();
    }

    public void OnSelectionChanged()
    {
        if (SelectedCustomerBranch is not null)
        {
            SelectedCustomerBranch.Kunden_ID = _selectedBranch.Kunden_ID;
            _advertisementAreaStatisticsNearestCustomerBranches = GetAdvertisementAreaStatistics(SelectedCustomerBranch);
            AdvertisementAreaStatistics = FilterAdvertisementAreaStatistics(_advertisementAreaStatisticsNearestCustomerBranches, _currentNeigborRadioButtonText.Equals("Alle") == true ? null : _currentNeigborRadioButtonText);
        }
    }

    public void OnMouseDoubleClick()
    {
        SetAdvertisementAreaGeometry(AdvertisementAreaStatistics, SelectedAdvertisementAreaStatistics.Werbegebiets_Nr);
    }

    #endregion

    #region Private Methods

    private void SetNearestCustomerBranchData()
    {
        _customerBranchesCollection = CustomerBranches = _customerRepository.GetNearestCustomerBranches(_selectedBranch.Kunden_ID, _selectedBranch);
    }
    private void OnAreaChanged(string parameter)
    {
        _currentNeigborRadioButtonText = parameter;
        AdvertisementAreaStatistics = FilterAdvertisementAreaStatistics(_advertisementAreaStatisticsNearestCustomerBranches, _currentNeigborRadioButtonText.Equals("Alle") == true ? null : _currentNeigborRadioButtonText);
    }
    private List<AdvertisementAreaStatistics> GetAdvertisementAreaStatistics(CustomerBranch branch) => _advertisementAreaStatisticsRepository.GetCustomerStatisticsByBranch(branch);
    private List<AdvertisementAreaStatistics> FilterAdvertisementAreaStatistics(List<AdvertisementAreaStatistics> adAreaStatistics, string state)
    {
        if (state is not null)
            return adAreaStatistics.Where(a => a.Werbegebietsstatus == state).ToList();

        return adAreaStatistics;
    }
    private void SetAdvertisementAreaGeometry(List<AdvertisementAreaStatistics> areaStatistics, int advertismentAreaNumber)
    {
        var advertisementAreaGeometries = _advertisementAreaGeometryRepository.GetAdvertisementAreaGeometryByNumber(advertismentAreaNumber);
        var customerBranchText = AdvertisementAreaLayerNameBuilder(_selectedBranch, SelectedCustomerBranch, SelectedAdvertisementAreaStatistics.Gebietsbezeichnung);
        _mapManager.CreateAdvertisementAreaLayer(advertisementAreaGeometries, _selectedBranch.Kundenname, customerBranchText);
        _geometryRepositoryGebietsassistent.ExecuteCreateAdvertisementGeographyStoredProcedure(advertismentAreaNumber);
    }
    private string AdvertisementAreaLayerNameBuilder(CustomerBranch branch, CustomerBranch neigborBranch, string advertisementAreaText)
    {
        StringBuilder builder = new();
        if (neigborBranch is not null)
            builder.Append(neigborBranch.Filialname);
        else
            builder.Append(branch.Filialname);

        builder.Append($" {advertisementAreaText}");
        var text = Regex.Replace(builder.ToString(), "[^a-zA-Z0-9 .]", "");
        text = text.Replace(" ", "_");
        return text.Replace(".", "_");
    }


    #endregion

}
