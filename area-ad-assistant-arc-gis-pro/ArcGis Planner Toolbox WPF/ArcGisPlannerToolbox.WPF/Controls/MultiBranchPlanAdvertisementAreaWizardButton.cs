using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisPlannerToolbox.WPF.Events;
using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.Views;
using Autofac;

namespace ArcGisPlannerToolbox.WPF.Controls;

public class MultiBranchPlanAdvertisementAreaWizardButton : Button
{
    private MultiBranchPlanAdvertisementAreaWizard _window;
    private SubscriptionToken _wizardControlCloseSubscriptionToken;

    protected override void OnClick()
    {
        if (_window is null)
        {
            _wizardControlCloseSubscriptionToken = WizardClosedEvent.Subscribe(OnWindowsClosed);
            _window = App.Container.Resolve<MultiBranchPlanAdvertisementAreaWizard>();
            _window.Owner = FrameworkApplication.Current.MainWindow;
            _window.Closed += (o, e) =>
            {
                _window = null;
                WizardClosedEvent.Unsubscribe(_wizardControlCloseSubscriptionToken);
            };
            _window.Show();
        }
        else
        {
            if (_window.WindowState == System.Windows.WindowState.Minimized)
                _window.WindowState = System.Windows.WindowState.Normal;
            _window.Activate();
        }
    }
    private void OnWindowsClosed(bool args)
    {
        if (args)
        {
            _window?.Close();
        }
    }
}
