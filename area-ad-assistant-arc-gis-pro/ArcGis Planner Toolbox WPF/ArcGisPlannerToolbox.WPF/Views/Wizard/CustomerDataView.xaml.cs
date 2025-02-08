using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class CustomerDataView : UserControl
{
    public CustomerDataView()
    {
        DataContext = App.Container.Resolve<CustomerDataViewModel>();
        InitializeComponent();
    }
    public CustomerDataView(CustomerDataViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
