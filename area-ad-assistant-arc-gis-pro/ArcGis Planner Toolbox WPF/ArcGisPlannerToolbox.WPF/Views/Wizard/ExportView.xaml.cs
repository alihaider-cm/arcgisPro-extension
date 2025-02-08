using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class ExportView : UserControl
{
    public ExportView()
    {
        DataContext = App.Container.Resolve<ExportViewModel>();
        InitializeComponent();
    }
    public ExportView(ExportViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
