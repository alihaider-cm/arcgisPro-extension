using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.ViewModels;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class SubTourWindow : ProWindow
{
    public SubTourWindow(SubTourWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
