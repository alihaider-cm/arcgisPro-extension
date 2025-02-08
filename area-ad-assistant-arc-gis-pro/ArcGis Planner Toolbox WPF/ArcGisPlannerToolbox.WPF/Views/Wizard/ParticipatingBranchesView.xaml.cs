using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class ParticipatingBranchesView : UserControl
{
    public ParticipatingBranchesView()
    {
        DataContext = App.Container.Resolve<ParticipatingBranchesViewModel>();
        InitializeComponent();
    }
    public ParticipatingBranchesView(ParticipatingBranchesViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
