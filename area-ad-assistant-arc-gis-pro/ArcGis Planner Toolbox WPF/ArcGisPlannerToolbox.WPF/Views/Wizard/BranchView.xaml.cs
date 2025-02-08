using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class BranchView : UserControl
{
    public BranchView()
    {
        DataContext = App.Container.Resolve<BranchViewModel>();
        InitializeComponent();
    }
    public BranchView(BranchViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();   
    }
}
