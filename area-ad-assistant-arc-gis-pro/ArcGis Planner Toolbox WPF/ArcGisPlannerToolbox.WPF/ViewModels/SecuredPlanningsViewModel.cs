using ArcGIS.Core.Events;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Domain;
using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;
using System.Linq;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class SecuredPlanningsViewModel : WizardPageViewModel
{
    private readonly IPlanningRepository _planningRepository;
    private readonly SubscriptionToken _customerChangedSubscriptionToken;
    private List<PlanningData> _planningSource = new();

    private List<PlanningData> _Planning = new();
    public List<PlanningData> Plannings
    {
        get { return _Planning; }
        set { _Planning = value; OnPropertyChanged(); }
    }

    private PlanningData _selectedPlanning = new();
    public PlanningData SelectedPlanning
    {
        get { return _selectedPlanning; }
        set { _selectedPlanning = value; OnPropertyChanged(); }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get { return _searchText; }
        set { _searchText = value; OnPropertyChanged(); }
    }

    public int SelectedCustomerId { get; set; }

    public SecuredPlanningsViewModel(IPlanningRepository planningRepository)
    {
        _planningRepository = planningRepository;
        _customerChangedSubscriptionToken = MultiBranchWizardSteps.CustomerChanged.Subscribe(x => SelectedCustomerId = x.Kunden_ID);
    }

    public void OnLoaded()
    {
        Heading = "Gesicherte Planungen";
        NextStep = 8;
        _planningSource = Plannings = GetFormerPlannings();
        if (!string.IsNullOrEmpty(SearchText))
        {
            AllowNext = false;
            OnTextChanged();
        }
            
    }
    public void OnTextChanged()
    {
        if (SearchText.Length > 0)
        {
            var filteredPlanningData = FilterPlanningDataByTextInput();
            Plannings = null;
            Plannings = filteredPlanningData;
        }
        else
            Plannings = GetFormerPlannings();
    }
    public void OnSelectionChanged()
    {
        if (SelectedPlanning is not null)
        {
            AllowNext = true;
            MultiBranchWizardSteps.SecuredPlanningSelectionChanged.Publish(SelectedPlanning);
        }
    }

    private List<PlanningData> GetFormerPlannings()
    {
        return _planningRepository.GetFormerPlannings(SelectedCustomerId, nameof(PlanningType.Großraumplanung)).OrderByDescending(x => x.Planungs_Nr).ToList();
    }
    private List<PlanningData> FilterPlanningDataByTextInput()
    {
        List<PlanningData> filtered = new List<PlanningData>();
        if (_planningSource.Count > 0)
        {
            var input = SearchText.ToLower();
            if (int.TryParse(input, out int result))
            {
                filtered = _planningSource.Where(
                    x => x.Planungs_Nr == result
                    || (x.Kunden_ID == result)).ToList();
            }
            else
            {
                filtered = _planningSource.Where
                (x => x.Planung_von == null || x.Planung_von.ToLower().Contains(input)
                || (x.Planungstyp == null || x.Planungstyp.ToLower().Contains(input)))
                .ToList();
            }
        }
        return filtered;
    }
}
