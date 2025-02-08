using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class MapSelectionView : UserControl
{
    public MapSelectionView()
    {
        DataContext = App.Container.Resolve<MapSelectionViewModel>();
        InitializeComponent();
    }
    public MapSelectionView(MapSelectionViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
