using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class DetailinformationenViewModel : WizardPageViewModel
{
    #region Private Fields

    private readonly SubscriptionToken _customerChangedSubscriptionToken;
    private readonly IAnalysisRepository _analysisRepository;
    private readonly IPlanningRepository _planningRepository;
    private DateTime _plannerStartTime;

    #endregion

    #region Public Properties

    private List<string> _planningLevels = new();
    public List<string> PlanningLevels
    {
        get { return _planningLevels; }
        set { _planningLevels = value; OnPropertyChanged(); }
    }

    private string _selectedPlanningLevel = string.Empty;
    public string SelectedPlanningLevel
    {
        get { return _selectedPlanningLevel; }
        set { _selectedPlanningLevel = value; OnPropertyChanged(); }
    }

    private bool _isPlanningEnable = false;
    public bool IsPlanningEnable
    {
        get { return _isPlanningEnable; }
        set { _isPlanningEnable = value; OnPropertyChanged(); }
    }

    private Visibility _controlsVisibility = Visibility.Collapsed;
    public Visibility ControlsVisibility
    {
        get { return _controlsVisibility; }
        set { _controlsVisibility = value; OnPropertyChanged(); }
    }

    private List<Analysis> _analyses = new();
    public List<Analysis> Analyses
    {
        get { return _analyses; }
        set { _analyses = value; OnPropertyChanged(); }
    }

    private Analysis _selectedAnalysis = new();
    public Analysis SelectedAnalysis
    {
        get { return _selectedAnalysis; }
        set
        {
            _selectedAnalysis = value;
            OnPropertyChanged();
            if (_selectedAnalysis is not null)
                OnSelectedAnalysisChanged();
        }
    }

    private double _customerPercentage = 3;
    public double CustomerPercentage
    {
        get { return _customerPercentage; }
        set { _customerPercentage = value; _customerPercentage = Math.Round(_customerPercentage, MidpointRounding.ToEven); OnPropertyChanged(); }
    }

    private bool _nonParticipatingBranches;
    public bool NonParticipatingBranches
    {
        get { return _nonParticipatingBranches; }
        set { _nonParticipatingBranches = value; OnPropertyChanged(); }
    }

    private double _progress;
    public double Progress
    {
        get { return _progress; }
        set { _progress = value; OnPropertyChanged(); }
    }

    private string _progressStatus = "Plane Gebiete ...";
    public string ProgressStatus
    {
        get { return _progressStatus; }
        set { _progressStatus = value; OnPropertyChanged(); }
    }

    public int SelectedCustomerId { get; set; }
    public ICommand StartPlannerCommand { get; set; }

    #endregion

    #region Initialization

    public DetailinformationenViewModel(IAnalysisRepository analysisRepository, IPlanningRepository planningRepository)
    {
        _analysisRepository = analysisRepository;
        _planningRepository = planningRepository;
        PlanningLevels = Enum.GetNames<PlanningLevels>().ToList();
        SelectedPlanningLevel = nameof(Core.Models.PlanningLevels.EMS);
        IsPlanningEnable = false;

        StartPlannerCommand = new RelayCommand(OnSetupAutoPlannerTracking);
        _customerChangedSubscriptionToken = MultiBranchWizardSteps.CustomerChanged.Subscribe(x => SelectedCustomerId = x.Kunden_ID);

    }

    #endregion

    #region Public Methods

    public void OnLoaded()
    {
        Heading = "Detailinformationen";
        NextStep = 8;
        if (ProgressStatus.Equals("Fertig!"))
            AllowNext = true;
        else
            AllowNext = false;

        Analyses = _analysisRepository.GetAnalysisByCustomerId(SelectedCustomerId);
        SelectedAnalysis = Analyses.FirstOrDefault(x => x.Analyse_ID == Analyses.Max(x => x.Analyse_ID));
    }
    public void OnPlanningLevelSelectionChanged()
    {
        if (SelectedPlanningLevel.Equals("BBE"))
            ControlsVisibility = Visibility.Visible;
        else
            ControlsVisibility = Visibility.Collapsed;
    }

    #endregion

    #region Private Methods

    private async Task OnSetupAutoPlannerTracking()
    //private void OnSetupAutoPlannerTracking()
    {
        ProgressStatus = "Plane Gebiete ...";
        Progress = 0;
        DateTime nowTime = DateTime.Now;
        _plannerStartTime = SubtracktFromDateTime(nowTime, TimeSpan.FromMinutes(1));
        var _currentPlanningNumber = _planningRepository.GetCurrentPlanningNumber(_plannerStartTime) + 1;
        PlanningNumberChangedEvent.Publish(_currentPlanningNumber);
        /*_planningRepository.ExecuteAutoPlannerStoredProcedureAsync(
                SelectedAnalysis.Analyse_ID,
                SelectedPlanningLevel,
                NonParticipatingBranches,
                Convert.ToInt32(CustomerPercentage)); */
        var _ = _planningRepository.ExecuteAutoPlannerStoredProcedureAsync(
                SelectedAnalysis.Analyse_ID,
                SelectedPlanningLevel,
                NonParticipatingBranches,
                Convert.ToInt32(CustomerPercentage));
        await TrackAutoPlannerProgress();
        ProgressStatus = "Fertig!";
        AllowNext = true;
    }
    private DateTime SubtracktFromDateTime(DateTime dt, TimeSpan ts) => new DateTime(dt.Ticks - ts.Ticks, dt.Kind);
    private async Task TrackAutoPlannerProgress()
    {
        int processSteps = _planningRepository.GetBranchesToBePlannedCount(SelectedCustomerId);
        int processedBranchesCount = 0;
        decimal percentage;
        while (processedBranchesCount < processSteps)
        {
            processedBranchesCount = await _planningRepository.GetCurrentlyPlannedBranchesCountAsync(_plannerStartTime);
            percentage = (processedBranchesCount * 100) / (processSteps + 1);
            Progress = Convert.ToDouble(Math.Floor(percentage));
        }
        int finishedBranchesCount = 0;
        while (finishedBranchesCount == 0)
        {
            finishedBranchesCount = _planningRepository.GetFinishedBranchesCount(_plannerStartTime);
            if (finishedBranchesCount > 0)
                Progress = 100;
        }
    }
    private void OnSelectedAnalysisChanged() => MultiBranchWizardSteps.AnalysisIdChanged.Publish(SelectedAnalysis.Analyse_ID);

    #endregion
}
