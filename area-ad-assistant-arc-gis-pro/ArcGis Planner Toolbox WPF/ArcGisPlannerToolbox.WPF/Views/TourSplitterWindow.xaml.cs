using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.ViewModels;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class TourSplitterWindow : ProWindow
{
    public TourSplitterWindow(TourSplitterWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
