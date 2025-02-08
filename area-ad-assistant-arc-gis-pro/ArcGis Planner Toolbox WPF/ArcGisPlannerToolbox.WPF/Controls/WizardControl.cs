using ArcGisPlannerToolbox.WPF.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Controls;

public class WizardControl : Control
{
    public static readonly DependencyProperty CurrentStepProperty =
        DependencyProperty.Register(
            nameof(CurrentStep),
            typeof(int),
            typeof(WizardControl),
            new FrameworkPropertyMetadata(0)
        );

    public static readonly DependencyProperty StepsProperty =
        DependencyProperty.Register(
            nameof(Steps),
            typeof(ObservableCollection<UserControl>),
            typeof(WizardControl),
            new FrameworkPropertyMetadata(new ObservableCollection<UserControl>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                PagesPropertyChangedCallback));

    public int CurrentStep
    {
        get { return (int)GetValue(CurrentStepProperty); }
        set { SetValue(CurrentStepProperty, value); }
    }
    public ObservableCollection<UserControl> Steps
    {
        get { return (ObservableCollection<UserControl>)GetValue(StepsProperty); }
        set { SetValue(StepsProperty, value); }
    }
    static WizardControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WizardControl), new
            FrameworkPropertyMetadata(typeof(WizardControl)));
    }
    public WizardControl()
    {
        Steps = new ObservableCollection<UserControl>();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (DataContext is WizardControlViewModel viewModel)
        {
            viewModel.Steps = Steps;
        }
    }

    private static void PagesPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update UI based on current step (optional)
        if (d is WizardControl wizardControl && wizardControl.DataContext is WizardControlViewModel viewModel)
        {
            viewModel.Steps = new ObservableCollection<UserControl>(wizardControl.Steps);
        }
    }
}
