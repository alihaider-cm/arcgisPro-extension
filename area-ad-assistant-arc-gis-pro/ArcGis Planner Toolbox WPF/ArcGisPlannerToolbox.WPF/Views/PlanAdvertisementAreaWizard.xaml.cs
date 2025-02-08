using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class PlanAdvertisementAreaWizard : ProWindow
{
    public PlanAdvertisementAreaWizard()
    {
        DataContext = App.Container.Resolve<PlanAdvertisementAreaWizardViewModel>();
        InitializeComponent();
    }
    public PlanAdvertisementAreaWizard(PlanAdvertisementAreaWizardViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
