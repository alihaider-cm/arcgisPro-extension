using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models.DTO;
using ArcGisPlannerToolbox.WPF.Helpers;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class ParticipatingBranchesView1ViewModel : WizardPageViewModel
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPlanningRepository _planningRepository;
    private readonly ICursorService _cursorService;
    private readonly SubscriptionToken _customerChangedSubscriptionToken;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get { return _searchText; }
        set { _searchText = value; OnPropertyChanged(); }
    }

    private int _auflage;
    public int Auflage
    {
        get { return _auflage; }
        set { _auflage = value; OnPropertyChanged(); }
    }

    private List<CustomerBranchDTO> _customerBranches = new();
    public List<CustomerBranchDTO> CustomerBranches
    {
        get { return _customerBranches; }
        set { _customerBranches = value; OnPropertyChanged(); }
    }

    private double _progress = 0;
    public double Progress
    {
        get { return _progress; }
        set { _progress = value; OnPropertyChanged(); }
    }

    public int SelectedCustomerId { get; set; }
    public ICommand BranchesTargetPointCommand { get; set; }
    public ICommand UploadToDatabaseCommand { get; set; }
    public ICommand CancelSelectionCommand { get; set; }


    public ParticipatingBranchesView1ViewModel(ICustomerRepository customerRepository, IPlanningRepository planningRepository, ICursorService cursorService)
    {
        _customerRepository = customerRepository;
        _planningRepository = planningRepository;
        _cursorService = cursorService;

        CancelSelectionCommand = new RelayCommand(OnCancelSelection);
        BranchesTargetPointCommand = new RelayCommand(OnButtonClick);
        UploadToDatabaseCommand = new RelayCommand(OnExecuteUpload);
        _customerChangedSubscriptionToken = MultiBranchWizardSteps.CustomerChanged.Subscribe(x => SelectedCustomerId = x.Kunden_ID);
    }

    public void OnTextChanged()
    {
        if (SearchText.Length > 0)
        {
            var filterColumns = new List<string>() { "Filialname", "Filial_Nr", "ORT", "Straße", "PLZ" };
            var branches = CustomerBranches.FilterGenericListByTextInput(SearchText, filterColumns);
            CustomerBranches = null;
            CustomerBranches = branches;
        }
        else
        {
            CustomerBranches = null;
            CustomerBranches = GetCustomerBranches();
        }
    }
    public void OnWindowLoaded()
    {
        Heading = "Teilnehmende Filialen";
        CustomerBranches = GetCustomerBranches();
        NextStep = 6;
        AllowNext = true;
    }

    private void OnButtonClick()
    {
        foreach (var branch in CustomerBranches)
            branch.Auflage = Auflage;
    }
    private async Task OnExecuteUpload()
    {
        try
        {
            ProApp.Current.MainWindow.Cursor = Cursors.Wait;
            _cursorService.SetCursor(Cursors.Wait);
            var uniqueBranches = CustomerBranches.Distinct().ToList();
            int currentItem = 1;
            foreach (var branch in uniqueBranches)
            {
                await _planningRepository.ExecuteUploadBranchDataToDatabase(SelectedCustomerId, branch);
                Progress = CalculateProgress(currentItem, uniqueBranches.Count);
                currentItem++;
            }
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
            _cursorService.SetCursor(Cursors.Wait);
        }
        catch (Exception ex)
        {
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
            _cursorService.SetCursor(Cursors.Arrow);
        }
        AllowNext = true;
    }
    private double CalculateProgress(int current, int total)
    {
        double value = (current * 100d) / total;
        return Math.Round(value, 2);
    }
    private List<CustomerBranchDTO> GetCustomerBranches()
    {
        var branches = _customerRepository.GetBranchByCustomer(SelectedCustomerId);
        return branches.ConvertAll<CustomerBranchDTO>(x => new(x));
    }
    private void OnCancelSelection()
    {
        CustomerBranches.ForEach(x => x.IsChecked = false);
    }
}
