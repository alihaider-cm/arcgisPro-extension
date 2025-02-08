using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class PreliminaryBranchAreasView : UserControl
{
    public PreliminaryBranchAreasView()
    {
        DataContext = App.Container.Resolve<PreliminaryBranchAreasViewModel>();
        InitializeComponent();
    }
    public PreliminaryBranchAreasView(PreliminaryBranchAreasViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
