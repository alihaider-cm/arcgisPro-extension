using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class PlanningStatisticsView : UserControl
{
    public PlanningStatisticsView()
    {
        DataContext = App.Container.Resolve<PlanningStatisticsViewModel>();
        InitializeComponent();
    }
    public PlanningStatisticsView(PlanningStatisticsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
