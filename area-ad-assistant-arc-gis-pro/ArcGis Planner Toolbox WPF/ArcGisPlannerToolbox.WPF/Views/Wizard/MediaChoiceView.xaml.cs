using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Views.Wizard;

public partial class MediaChoiceView : UserControl
{
    public MediaChoiceView()
    {
        DataContext = App.Container.Resolve<MediaChoiceViewModel>();
        InitializeComponent();
    }
    public MediaChoiceView(MediaChoiceViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
