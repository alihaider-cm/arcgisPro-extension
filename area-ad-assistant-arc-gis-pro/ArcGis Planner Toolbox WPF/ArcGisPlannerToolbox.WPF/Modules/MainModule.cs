using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Events;
using ArcGisPlannerToolbox.WPF.Constants;
using ArcGisPlannerToolbox.WPF.Startup;
using System;

namespace ArcGisPlannerToolbox.WPF.Modules;

public class MainModule : Module
{
    private const string _moduleName = DamlConfig.Module.MainModule;
    public MainModule()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzM1MjEwNkAzMjM1MmUzMDJlMzBTK0JpUDEyYjB1djdURkpmVURSYlAwamFiM01aM3VRMStDbmllN1ZiMnJNPQ==");
        ApplicationStartupEvent.Subscribe(x => OnApplicationStartup(x));
    }

    private void OnApplicationStartup(EventArgs x) => App.RegisterDependencies();

    private static MainModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static MainModule Current => _this ??= (MainModule)FrameworkApplication.FindModule(_moduleName);

    #region Overrides

    //protected override void OnUpdate()
    //{
    //    base.OnUpdate();
    //}

    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
        //TODO - add your business logic
        //return false to ~cancel~ Application close
        return true;
    }

    #endregion Overrides
}
