using System;

namespace ArcGisPlannerToolbox.WPF.Constants.Settings;

public class UserProfile
{
    private static readonly Lazy<UserProfile> _lazy = new Lazy<UserProfile>(() => new UserProfile());
    public static UserProfile Current { get { return _lazy.Value; } }
    public FilterMediaDockSettings FilterMediaDockSettings;
    private UserProfile()
    {
        FilterMediaDockSettings = new FilterMediaDockSettings();
    }
}
