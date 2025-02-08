using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MultiBranchWizardSteps = ArcGisPlannerToolbox.WPF.Events.MultiBranchPlanAdvertisementAreaWizardStepsCompleted;
namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class ParticipatingBranchesViewModel : WizardPageViewModel
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPlanningRepository _planningRepository;
    private readonly ICursorService _cursorService;

    private readonly SubscriptionToken _customerChangedSubscriptionToken;

    private int _auflage;
    public int Auflage
    {
        get { return _auflage; }
        set { _auflage = value; OnPropertyChanged(); }
    }

    private List<CustomerBranch> _customerBranches = new();
    public List<CustomerBranch> CustomerBranches
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

    public ParticipatingBranchesViewModel(ICustomerRepository customerRepository, IPlanningRepository planningRepository, ICursorService cursorService)
    {
        _customerRepository = customerRepository;
        _planningRepository = planningRepository;
        _cursorService = cursorService;

        BranchesTargetPointCommand = new RelayCommand(OnButtonClick);
        UploadToDatabaseCommand = new RelayCommand(OnExecuteUpload);
        _customerChangedSubscriptionToken = MultiBranchWizardSteps.CustomerChanged.Subscribe(x => SelectedCustomerId = x.Kunden_ID);
    }

    public void OnWindowLoaded()
    {
        Heading = "Teilnehmende Filialen";
        CustomerBranches = _customerRepository.GetBranchByCustomer(SelectedCustomerId);
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
            _cursorService.SetCursor(Cursors.Arrow);
        }
        catch (Exception ex)
        {
            ProApp.Current.MainWindow.Cursor = Cursors.Arrow;
            _cursorService.SetCursor(Cursors.Arrow);
        }
        
    }
    private double CalculateProgress(int current, int total)
    {
        double value = (current * 100d) / total;
        return Math.Round(value, 2);
    }
}
