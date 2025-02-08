using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.ViewModels;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class DigitizedMediaStatusWindow : ProWindow
{
    public DigitizedMediaStatusWindow(DigitizedMediaStatusWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
