using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.Views;
using Autofac;

namespace ArcGisPlannerToolbox.WPF.Controls;

public class MediaDigitizerWindowButton : Button
{
    private MediaDigitizerWindow _window;
    protected override void OnClick()
    {
        if (_window is null)
        {
            _window = App.Container.Resolve<MediaDigitizerWindow>();
            _window.Owner = FrameworkApplication.Current.MainWindow;
            _window.Closed += (o, e) => { _window = null; };
            _window.Show();
        }
        else
        {
            if (_window.WindowState == System.Windows.WindowState.Minimized)
                _window.WindowState = System.Windows.WindowState.Normal;
            _window.Activate();
        }
    }
}
