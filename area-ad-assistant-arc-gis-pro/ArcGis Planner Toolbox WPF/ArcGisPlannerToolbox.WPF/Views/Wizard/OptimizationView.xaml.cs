using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class OptimizationView : UserControl
{
    public OptimizationView()
    {
        DataContext = App.Container.Resolve<OptimizationViewModel>();
        InitializeComponent();
    }
    public OptimizationView(OptimizationViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
