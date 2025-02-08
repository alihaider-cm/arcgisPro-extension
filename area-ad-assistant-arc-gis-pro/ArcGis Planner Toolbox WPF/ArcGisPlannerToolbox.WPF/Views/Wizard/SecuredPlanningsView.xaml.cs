using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class SecuredPlanningsView : UserControl
{
    public SecuredPlanningsView()
    {
        DataContext = App.Container.Resolve<SecuredPlanningsViewModel>();
        InitializeComponent();
    }
    public SecuredPlanningsView(SecuredPlanningsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
