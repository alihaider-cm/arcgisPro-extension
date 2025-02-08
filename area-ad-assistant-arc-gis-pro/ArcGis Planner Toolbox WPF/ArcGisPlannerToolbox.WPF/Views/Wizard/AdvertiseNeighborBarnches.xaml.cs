using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class AdvertiseNeighborBarnches : UserControl
{
    public AdvertiseNeighborBarnches()
    {
        DataContext = App.Container.Resolve<AdvertiseNeighborBarnchesViewModel>();
        InitializeComponent();
    }

    public AdvertiseNeighborBarnches(AdvertiseNeighborBarnchesViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
