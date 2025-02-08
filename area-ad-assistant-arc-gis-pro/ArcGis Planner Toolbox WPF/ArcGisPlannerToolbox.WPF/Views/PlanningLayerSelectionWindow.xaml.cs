using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.ViewModels;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class PlanningLayerSelectionWindow : ProWindow
{
    public PlanningLayerSelectionWindow()
    {
        InitializeComponent();
    }
    public PlanningLayerSelectionWindow(PlanningLayerSelectionWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
