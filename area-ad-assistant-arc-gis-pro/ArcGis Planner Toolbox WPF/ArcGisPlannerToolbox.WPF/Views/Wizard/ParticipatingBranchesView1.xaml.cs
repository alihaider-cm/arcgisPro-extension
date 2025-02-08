using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class ParticipatingBranchesView1 : UserControl
{
    public ParticipatingBranchesView1()
    {
        DataContext = App.Container.Resolve<ParticipatingBranchesView1ViewModel>();
        InitializeComponent();
    }
    public ParticipatingBranchesView1(ParticipatingBranchesView1ViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
