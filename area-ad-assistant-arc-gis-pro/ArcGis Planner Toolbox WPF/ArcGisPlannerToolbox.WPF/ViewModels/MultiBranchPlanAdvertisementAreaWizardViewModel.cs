using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class MultiBranchPlanAdvertisementAreaWizardViewModel : BindableBase
{
    private readonly IMapManager _mapManager;
    private readonly IPlanningRepository _planningRepository;
    public WizardControlViewModel WizardControlViewModel { get; init; }
    public ICommand ClosingCommand { get; set; }
    public MultiBranchPlanAdvertisementAreaWizardViewModel(IMapManager mapManager, IPlanningRepository planningRepository, WizardControlViewModel wizardControlViewModel)
    {
        _mapManager = mapManager;
        _planningRepository = planningRepository;
        WizardControlViewModel = wizardControlViewModel;
        ClosingCommand = new RelayCommand<CancelEventArgs>(OnWindowClosing);
    }
    public async void OnWindowLoaded()
    {
        if (_mapManager.CheckIfZipCodeLayerExists())
        {
            await _mapManager.SetZipCodeLayer();
            if (!await _mapManager.CheckIfZipCodeLayerHasDBConnection())
            {
                await _mapManager.SetZipCodeLayerVisibility(false);
                await _mapManager.ConnectZipCodeToDataBase();
            }
        }
    }
    public async void OnWindowClosed()
    {
        _planningRepository.ExecuteStoredProcedurePlanningSelectionTable("", 0, 0);
        await _mapManager.ClearMapTreeView();
    }
    public void OnWindowClosing(CancelEventArgs args)
    {
        var dialogResult = MessageBox.Show("Möchten sie die aktuelle Planung wirklich beenden?", "Planung beenden", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (dialogResult == MessageBoxResult.No)
            args.Cancel = true;
    }
}
