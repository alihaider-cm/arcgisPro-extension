using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Reports;
using ArcGisPlannerToolbox.Core.Contracts;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class OptimizationViewModel : WizardPageViewModel
{
    private readonly IPlanningRepository _planningRepository;

    private double _tourCoverage = 30;
    public double TourCoverage
    {
        get { return _tourCoverage; }
        set
        {
            _tourCoverage = value;
            _tourCoverage = Math.Round(_tourCoverage, MidpointRounding.ToEven);
            OnPropertyChanged();
        }
    }
    private double _percentage;
    public double Percentage
    {
        get { return _percentage; }
        set
        {
            _percentage = value;
            OnPropertyChanged();
        }
    }

    public ICommand TourCoverageCommand { get; set; }
    public OptimizationViewModel(IPlanningRepository planningRepository)
    {
        _planningRepository = planningRepository;
        TourCoverageCommand = new RelayCommand(SetupTourCovareageTracking);
    }
    public void OnLoaded()
    {
        Heading = "Medien- und Tourenoptimierung";
        NextStep = 11;
    }
    private async Task SetupTourCovareageTracking()
    {
        await Task.Run(async () =>
        {
            int processSteps = 3;
            int currentStep = 1;
            decimal percentage;

            decimal coverage = (decimal)_tourCoverage / (decimal)100;
            percentage = (currentStep * 100) / (processSteps);
            Percentage = Convert.ToInt32(Math.Floor(percentage));
            await Task.Delay(100);
            _planningRepository.ExecuteReplaceInsufficientMediaStoredProcedure(coverage);
            currentStep = 2;
            percentage = (currentStep * 100) / (processSteps);
            Percentage = Convert.ToInt32(Math.Floor(percentage));
            await Task.Delay(100);
            _planningRepository.ExecuteFinalResultStoredProcedure(coverage);
            currentStep = 3;
            percentage = (currentStep * 100) / (processSteps);
            Percentage = Convert.ToInt32(Math.Floor(percentage));
            AllowNext = true;
        });
    }
}
