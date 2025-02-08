using ArcGIS.Core.Events;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Events;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class PlanningStatisticsViewModel : WizardPageViewModel
{
    #region Private Fields

    private readonly IPlanningRepository _planningRepository;
    private readonly SubscriptionToken _formerPlanningChangedSubscriptionToken;
    private readonly SubscriptionToken _securedPlanningSelectionChangedSubscriptionToken;
    private List<PlanningData> _planningData;
    private int _currentPlanningNumber;
    private PlanningData _securedPlanningItem;

    #endregion

    #region Public Properties

    private List<PlanningStatistic> _planningStatistics = new();
    public List<PlanningStatistic> PlanningStatistics
    {
        get { return _planningStatistics; }
        set { _planningStatistics = value; OnPropertyChanged(); }
    }
    public ObservableCollection<double> ChartValues { get; set; } = new();
    public ObservableCollection<string> ChartLabels { get; set; } = new();
    public Axis[] XAxes { get; set; }
    private ISeries[] _series;
    public ISeries[] Series { get { return _series; } set { _series = value; OnPropertyChanged(); } }
    public string SelectedOption { get; set; }

    #endregion

    #region Constructor

    public PlanningStatisticsViewModel(IPlanningRepository planningRepository)
    {
        XAxes = new Axis[]{
            new Axis
            {
                Labels = ChartLabels,
                LabelsRotation = 0,
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                SeparatorsAtCenter = false,
                TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
                TicksAtCenter = true,
                ForceStepToMin = true,
                MinStep = 1
            }
        };
        _series = new ISeries[] { new ColumnSeries<double> {
            Name = "Auflage",
            Values = ChartValues
        }};
        _planningRepository = planningRepository;
        _formerPlanningChangedSubscriptionToken = MultiBranchWizardSteps.FormerPlanningChanged.Subscribe(x => SelectedOption = x);
        _securedPlanningSelectionChangedSubscriptionToken = MultiBranchWizardSteps.SecuredPlanningSelectionChanged.Subscribe(x => _securedPlanningItem = x);
    }

    #endregion

    #region Public Methods

    public void OnLoaded()
    {
        Heading = "Planungstatistiken";
        NextStep = 9;
        AllowNext = true;

        Initialize();
    }

    #endregion

    #region Private Methods

    private void Initialize()
    {
        ChartLabels.Clear();
        ChartValues.Clear();
        if (!SelectedOption.Equals("OpenFormerPlanning"))
        {
            _planningData = GetPlanningData();
            _currentPlanningNumber = _planningData.Select(p => p.Planungs_Nr).FirstOrDefault();
            PlanningNumberChangedEvent.Publish(_currentPlanningNumber);
        }
        else if (SelectedOption.Equals("OpenFormerPlanning"))
        {
            if (_securedPlanningItem is not null)
            {
                _planningData = _planningRepository.GetPlanningDataByPlanningNumberAndConceptNumber(_securedPlanningItem.Planungs_Nr, _securedPlanningItem.Konzept_Nr);
                _currentPlanningNumber = _securedPlanningItem.Planungs_Nr;
                _planningRepository.FormerPlanningNumber = _securedPlanningItem.Planungs_Nr;
                PlanningNumberChangedEvent.Publish(_currentPlanningNumber);
            }
        }
        ShowStatistics();
        var bins = CalculateDataBins(_planningData);
        var dataSeries = GetDataSeries(bins, _planningData);
        foreach (var series in dataSeries)
        {
            ChartLabels.Add(series.Key);
            ChartValues.Add(series.Value);
        }
    }
    private List<PlanningData> GetPlanningData() => _planningRepository.GetPlanningData();
    private void ShowStatistics()
    {
        int totalPlanned = _planningData.Count;
        int notPlanned = _planningData.Where(p => p.Auflage == 0).Count();
        int targetEdition = _planningData.Select(p => p.Zielauflage1).Sum();
        int plannedEdition = _planningData.Select(p => p.Auflage).Sum();
        double std = CalulateSTDFromTargetEdition(_planningData);
        PlanningStatistic statistic = new PlanningStatistic()
        {
            TotalPlanned = totalPlanned,
            NotPlanned = notPlanned,
            StandardDeviation = std,
            TotalTargetEdition = targetEdition,
            TotalPlannedEdition = plannedEdition
        };
        PlanningStatistics = new List<PlanningStatistic>() { statistic };
    }
    private double CalulateSTDFromTargetEdition(List<PlanningData> data)
    {
        double result = 0;
        if (data.Any())
        {
            double sum = data.Sum(d => Math.Pow(d.Auflage - d.Zielauflage1, 2));
            result = Math.Sqrt((sum) / data.Count());
        }
        return result;
    }
    private Dictionary<int, Tuple<int, int>> CalculateDataBins(List<PlanningData> planningData)
    {
        var editionNumbers = planningData.Select(p => p.Auflage).ToList();
        int minVal = editionNumbers.Min();
        int maxVal = editionNumbers.Max();
        int steps = 7;
        var stepRange = GetBinSteps(minVal, maxVal, steps);
        var borders = GetBorders(minVal, maxVal, steps + 1, stepRange);
        return CreateBins(borders);
    }
    private int GetBinSteps(int minVal, int maxVal, int binCount) => Convert.ToInt32(Math.Ceiling((maxVal - minVal) / Convert.ToDouble(binCount)));
    private List<int> GetBorders(int minVal, int maxVal, int steps, int stepRange)
    {
        List<int> borders = new List<int>();
        int i = minVal;
        if (stepRange > 0)
        {
            for(int j = 0; j < steps; j++)
            {
                if (i == 0)
                {
                    borders.Add(i);
                    i = minVal + 1;
                }
                borders.Add(i);
                i += stepRange;
            }
        }
        else
        {
            borders.Add(minVal);
            borders.Add(maxVal);
        }
        return borders;
    }
    private Dictionary<int, Tuple<int, int>> CreateBins(List<int> borders)
    {
        Dictionary<int, Tuple<int, int>> bins = new Dictionary<int, Tuple<int, int>>();
        int counter = 1;
        int i = 0;
        while (i < borders.Count - 1)
        {
            var interval = new Tuple<int, int>(borders[i], borders[i + 1]);
            bins.Add(counter, interval);
            counter += 1;
            i += 1;
        }
        return bins;
    }
    private Dictionary<string, int> GetDataSeries(Dictionary<int, Tuple<int, int>> bins, List<PlanningData> data)
    {
        Dictionary<string, int> series = new Dictionary<string, int>();
        for (int i = 1; i <= bins.Count; i++)
        {
            int dataCount = data.Where(d => d.Auflage >= bins[i].Item1 && d.Auflage < bins[i].Item2)
                .Select(d => d.Auflage)
                .ToList()
                .Count();
            series.Add($"{bins[i].Item1} - {bins[i].Item2}", dataCount);
        }
        return series;
    }

    #endregion
}
