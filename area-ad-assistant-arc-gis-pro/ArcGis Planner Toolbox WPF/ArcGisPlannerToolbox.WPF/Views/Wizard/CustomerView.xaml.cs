using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class CustomerView : UserControl
{
    public CustomerView()
    {
        DataContext = App.Container.Resolve<CustomerViewModel>();
        InitializeComponent();
    }
    public CustomerView(CustomerViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
