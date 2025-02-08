using ArcGisPlannerToolbox.WPF.ViewModels;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class FilterMediaDock : UserControl
{
    public FilterMediaDock()
    {
        DataContext = FilterMediaDockViewModel.Instance;
        InitializeComponent();
    }
}
