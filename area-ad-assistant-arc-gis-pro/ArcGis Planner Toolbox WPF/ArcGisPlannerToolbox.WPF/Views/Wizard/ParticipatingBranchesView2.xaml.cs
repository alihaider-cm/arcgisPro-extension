using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class ParticipatingBranchesView2 : UserControl
{
    public ParticipatingBranchesView2()
    {
        DataContext = App.Container.Resolve<ParticipatingBranchesView2ViewModel>();
        InitializeComponent();
    }
}
