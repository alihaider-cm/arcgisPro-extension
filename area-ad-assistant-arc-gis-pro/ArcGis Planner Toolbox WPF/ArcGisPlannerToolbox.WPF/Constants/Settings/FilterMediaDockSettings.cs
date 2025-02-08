using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Constants.Settings;

public class FilterMediaDockSettings
{
    public readonly List<string> ZoomOptions;
    public string SelectedZoomOption { get; set; }   
    public bool AutoMapViewFilter { get; set; }
    public bool ShowMediaWithDistribution { get; set; }
    public bool CollapsedGroupBox { get; set; }
    public double SelectedTransparencyValues { get; set; }

    public FilterMediaDockSettings()
    {
        ZoomOptions = new List<string>() { Constants.ZoomOptions.All, Constants.ZoomOptions.LastLayer, Constants.ZoomOptions.Off };
    }

}
