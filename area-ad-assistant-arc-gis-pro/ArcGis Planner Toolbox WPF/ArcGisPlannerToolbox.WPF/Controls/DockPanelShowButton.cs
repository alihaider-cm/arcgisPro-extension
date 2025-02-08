using ArcGIS.Desktop.Framework.Contracts;
using ArcGisPlannerToolbox.WPF.Startup;
using ArcGisPlannerToolbox.WPF.ViewModels;
using Autofac;

namespace ArcGisPlannerToolbox.WPF.Controls;

public class DockPanelShowButton : Button
{
    protected override void OnClick()
    {
        FilterMediaDockViewModel.Show();
    }
}
