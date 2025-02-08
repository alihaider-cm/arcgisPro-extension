using ArcGIS.Desktop.Framework.Controls;
using ArcGisPlannerToolbox.WPF.ViewModels;

namespace ArcGisPlannerToolbox.WPF.Views;

public partial class AddBranchWindow : ProWindow
{
    public AddBranchWindow(AddBranchWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
