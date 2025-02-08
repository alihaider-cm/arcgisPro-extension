using ArcGIS.Core.Events;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class PreliminaryBranchAreasViewModel : WizardPageViewModel
{
    private readonly IPlanningRepository _planningRepository;
    private readonly SubscriptionToken _customerChangedSubscriptionToken;
    private List<string> _predefinedBranchNumbersReleasable = new();


    private List<PlanningDataArea> _preDefinedAreaList = new();
    public List<PlanningDataArea> PreDefinedAreaList
    {
        get { return _preDefinedAreaList; }
        set { _preDefinedAreaList = value; OnPropertyChanged(); }
    }

    private PredefinedBranch _selectedPredefinedBranch = new();
    public PredefinedBranch SelectedPredefinedBranch
    {
        get { return _selectedPredefinedBranch; }
        set { _selectedPredefinedBranch = value; OnPropertyChanged(); }
    }

    private List<PredefinedBranch> _predefinedBranches = new();
    public List<PredefinedBranch> PredefinedBranches
    {
        get { return _predefinedBranches; }
        set { _predefinedBranches = value; OnPropertyChanged(); }
    }
    
    public int SelectedCustomerId { get; set; }
    public ICommand FilialNrCheckedCommand { get; set; }
    public ICommand FilialNrUncheckedCommand { get; set; }
    public ICommand ReleaseButtonCommand { get; set; }
    public PreliminaryBranchAreasViewModel(IPlanningRepository planningRepository)
    {
        _planningRepository = planningRepository;
        FilialNrCheckedCommand = new RelayCommand<string>(OnFilialNrChecked);
        FilialNrUncheckedCommand = new RelayCommand<string>(OnFilialNrUnchecked);
        ReleaseButtonCommand = new RelayCommand(OnRelaseButtonClick);
        _customerChangedSubscriptionToken = MultiBranchWizardSteps.CustomerChanged.Subscribe(x => SelectedCustomerId = x.Kunden_ID);
    }

    public void OnLoaded()
    {
        Heading = "Initiale / vorläufige Filialgebiete";
        NextStep = 7;
        AllowNext = true;
        LoadPredefinedBranches();
    }
    public void LoadPredefinedBranches()
    {
        PredefinedBranches = _planningRepository.GetPredefinedBranches(SelectedCustomerId);
    }
    public void OnPredefinedBranchChanged()
    {
        if (SelectedPredefinedBranch is not null)
        {
            PreDefinedAreaList = new(_planningRepository.GetPredefinedArea(SelectedPredefinedBranch.Filial_Nr).Distinct());
        }
    }
    private void OnFilialNrChecked(string filialNr)
    {
        _predefinedBranchNumbersReleasable.Add(filialNr);
    }
    private void OnFilialNrUnchecked(string filialNr)
    {
        _predefinedBranchNumbersReleasable.Remove(filialNr);
    }
    private void OnRelaseButtonClick()
    {
        if(_predefinedBranchNumbersReleasable.Count > 0)
        {
            _planningRepository.ReleasePredefinedBranchNumbers(_predefinedBranchNumbersReleasable, SelectedCustomerId);
            LoadPredefinedBranches();
        }
    }
}
