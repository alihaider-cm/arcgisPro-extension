using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.ViewModels;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class MultiBranchPlanAdvertisementAreaWizard : ProWindow
{
    public MultiBranchPlanAdvertisementAreaWizard(MultiBranchPlanAdvertisementAreaWizardViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
