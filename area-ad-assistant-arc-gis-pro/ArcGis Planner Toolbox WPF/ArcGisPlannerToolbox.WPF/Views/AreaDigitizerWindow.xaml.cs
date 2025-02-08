using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.ViewModels;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class AreaDigitizerWindow : ProWindow
{
    public AreaDigitizerWindow(AreaDigitizerWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
