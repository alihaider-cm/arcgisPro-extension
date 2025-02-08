using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class ConceptDataView : UserControl
{
    public ConceptDataView()
    {
        DataContext = App.Container.Resolve<ConceptDataViewModel>();
        InitializeComponent();
    }

    public ConceptDataView(ConceptDataViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
