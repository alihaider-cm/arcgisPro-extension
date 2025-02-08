using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class DetailedPlanningView : UserControl
{
    public DetailedPlanningView()
    {
        DataContext = App.Container.Resolve<DetailedPlanningViewModel>();
        InitializeComponent();
    }
    public DetailedPlanningView(DetailedPlanningViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
