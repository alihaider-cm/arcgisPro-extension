using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class DetailinformationenView : UserControl
{
    public DetailinformationenView()
    {
        DataContext = App.Container.Resolve<DetailinformationenViewModel>();
        InitializeComponent();
    }
    public DetailinformationenView(DetailinformationenViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
