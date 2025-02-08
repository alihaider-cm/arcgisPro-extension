using ArcGisPlannerToolbox.WPF.Repositories.Contracts;

namespace ArcGisPlannerToolbox.WPF.ViewModels;

public class PlanAdvertisementAreaWizardViewModel : BindableBase
{
    private readonly IMapManager _mapManager;
    public WizardControlViewModel WizardControlViewModel { get; init; }
    public PlanAdvertisementAreaWizardViewModel(IMapManager mapManager, WizardControlViewModel wizardControlViewModel)
    {
        _mapManager = mapManager;
        WizardControlViewModel = wizardControlViewModel;
    }

    public async void OnWindowClosed()
    {
        await _mapManager.ClearMapTreeView();
    }
}
