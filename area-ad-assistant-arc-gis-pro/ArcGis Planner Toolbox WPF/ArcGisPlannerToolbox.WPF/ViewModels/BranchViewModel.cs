using ArcGIS.Core.Events;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using static ArcGisPlannerToolbox.WPF.Events.PlanAdvertisementAreaWizardEvent;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class BranchViewModel : WizardPageViewModel
{
    #region Fields

    private readonly IMapManager _mapManager;
    private readonly IGeometryRepositoryGebietsassistent _geometryRepositoryGebietsassistent;
    private readonly IAdvertisementAreaGeometryRepository _advertisementAreaGeometryRepository;
    private readonly IAdvertisementAreaRepository _advertisementAreaRepository;
    private readonly IAdvertisementAreaStatisticsRepository _advertisementAreaStatisticsRepository;
    private readonly SubscriptionToken _branchChangedToken;
    private SubscriptionToken _wizardPageToken;

    private List<AdvertisementAreaStatistics> _advertisementAreaStatisticsList = new();
    private List<AdvertisementAreaAreas> _advertisementAreaAreas = new();
    private bool _adAreaLoaded, _formerDataaccepted;
    private string _filter = string.Empty;
    //private CustomerBranch _selectedCustomerBranch;

    #endregion

    #region Properties

    private CustomerBranch _selectedBranch = new();
    public CustomerBranch SelectedBranch
    {
        get { return _selectedBranch; }
        set { _selectedBranch = value; OnPropertyChanged(); }
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


    public ICommand AreaChangedCommand { get; set; }

    #endregion

    #region Constructor

    public BranchViewModel(IAdvertisementAreaStatisticsRepository advertisementAreaStatisticsRepository,
                           IAdvertisementAreaRepository advertisementAreaRepository,
                           IMapManager mapManager,
                           IGeometryRepositoryGebietsassistent geometryRepositoryGebietsassistent,
                           IAdvertisementAreaGeometryRepository advertisementAreaGeometryRepository)
    {
        _mapManager = mapManager;
        _advertisementAreaRepository = advertisementAreaRepository;
        _geometryRepositoryGebietsassistent = geometryRepositoryGebietsassistent;
        _advertisementAreaGeometryRepository = advertisementAreaGeometryRepository;
        _advertisementAreaStatisticsRepository = advertisementAreaStatisticsRepository;

        AreaChangedCommand = new RelayCommand<string>(OnAreaChanged);
        _branchChangedToken = CustomerBranchChanged.Subscribe(x => SelectedBranch = x);
    }

    #endregion

    #region Public Methods

    public void OnLoaded()
    {
        _wizardPageToken = WizardPageChangedEvent.Subscribe(OnPageCommited);
        Heading = "Filiale";
        NextStep = 3;
        AllowNext = true;
        SetAdvertisementAreaStatistics();
        if (!string.IsNullOrWhiteSpace(_filter))
            OnAreaChanged(_filter);

    }
    public void OnSelectionChanged()
    {
        if (SelectedAdvertisementAreaStatistics is not null)
            BaseGeometryChanged.Publish(SelectedAdvertisementAreaStatistics.Basisgeometrie);
    }
    public void OnMouseDoubleClick()
    {
        // TODO: publish advertisementAreaNumber -- Subscribe on page4
        BaseGeometryChanged.Publish(SelectedAdvertisementAreaStatistics.Basisgeometrie);
        //SetSelectedAdvertisementAreaAreas(SelectedAdvertisementAreaStatistics.Werbegebiets_Nr);
        SetAdvertisementAreaGeometry(AdvertisementAreaStatistics, SelectedAdvertisementAreaStatistics.Werbegebiets_Nr);
        _adAreaLoaded = true;
    }

    #endregion

    #region Private Methods

    private void SetAdvertisementAreaStatistics()
    {
        _advertisementAreaStatisticsList = AdvertisementAreaStatistics = _advertisementAreaStatisticsRepository.GetCustomerStatisticsByBranch(SelectedBranch);
    }

    private void OnAreaChanged(string parameter)
    {
        _filter = parameter;
        if (parameter.Equals("Alle"))
            AdvertisementAreaStatistics = _advertisementAreaStatisticsList;
        else
            AdvertisementAreaStatistics = _advertisementAreaStatisticsList.Where(a => a.Werbegebietsstatus == parameter).ToList();
    }

    private void SetSelectedAdvertisementAreaAreas(int advertisementAreaNumber)
    {
        List<AdvertisementAreaAreas> areas = new List<AdvertisementAreaAreas>();
        if (SelectedAdvertisementAreaStatistics.Basisgeometrie == "BBE")
            areas.AddRange(_advertisementAreaRepository.GetAdvertisementAreaAreasByOccupancyUnitLevel(advertisementAreaNumber));
        else if (SelectedAdvertisementAreaStatistics.Basisgeometrie == "PLZ")
            areas.AddRange(_advertisementAreaRepository.GetAdvertisementAreaAreasByZipCodeLevel(advertisementAreaNumber));
        if (areas.Select(a => a.BBE_ID).Any())
            _advertisementAreaAreas = areas;
    }

    private void SetAdvertisementAreaGeometry(List<AdvertisementAreaStatistics> areaStatistics, int advertismentAreaNumber)
    {
        var advertisementAreaGeometries = _advertisementAreaGeometryRepository.GetAdvertisementAreaGeometryByNumber(advertismentAreaNumber);
        var customerBranchText = AdvertisementAreaLayerNameBuilder(SelectedBranch, null, SelectedAdvertisementAreaStatistics.Gebietsbezeichnung);
        _mapManager.CreateAdvertisementAreaLayer(advertisementAreaGeometries, SelectedBranch.Kundenname, customerBranchText);
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

    private void OnPageCommited(bool args)
    {
        if (!_adAreaLoaded && ValidateAdvertisementAreaStatistics())
        {
            AskUserForLoadingAreaInformation(SelectedAdvertisementAreaStatistics.Werbegebiets_Nr);
        }
    }

    private void AskUserForLoadingAreaInformation(int advertismentAreaNumber)
    {
        var dialogResult = MessageBox.Show("Möchten Sie Werbegebietsinformationen des selektierten Werbegebietes für diese Planung laden?" +
            " Wenn Sie mit Ja bestätigen, werden beispielsweise ehemals selektierte Flächen der selektierten Planung vorselektiert (bei Änderungen hilfreich).",
            "Übernahme von Werbegebietsinformationen", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (dialogResult == MessageBoxResult.Yes)
        {
            SetSelectedAdvertisementAreaAreas(advertismentAreaNumber);
            //SetSelectedAdvertisementAreaStatistics(advertismentAreaNumber);
            //planningLevel_cb.Enabled = false; // Publish event to Disable ComboBox
            _formerDataaccepted = true; // Publish to page 4
        }
    }

    private bool ValidateAdvertisementAreaStatistics()
    {
        if (string.IsNullOrWhiteSpace(SelectedAdvertisementAreaStatistics.Filialname) ||
            string.IsNullOrWhiteSpace(SelectedAdvertisementAreaStatistics.Kundenname) ||
            string.IsNullOrWhiteSpace(SelectedAdvertisementAreaStatistics.Basisgeometrie) ||
            string.IsNullOrWhiteSpace(SelectedAdvertisementAreaStatistics.Werbegebietsstatus))
        {
            return false;
        }
        return true;
    }

    #endregion

}
