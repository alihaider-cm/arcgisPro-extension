using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace ArcGisPlannerToolbox.WPF.Constants;

public static class AppConfig
{
    private static Configuration _configuration;
    static AppConfig()
    {
        Configure();
    }

    private static void Configure()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        var configFile = Path.Combine(path, "ArcPlannerToolbox", "App.config");
        ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
        configMap.ExeConfigFilename = configFile;

        _configuration = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
    }

    public static void GetEnvironment()
    {
        var environment = _configuration.AppSettings;
    }

    public static string GetConfiguration(string dataBaseName)
    {
        string connectionString = string.Empty;
        if (dataBaseName == "ANALYSEDATEN")
            connectionString = _configuration.ConnectionStrings.ConnectionStrings["ANALYSEDATEN"].ConnectionString;
        else if (dataBaseName == "MediaCenter")
            connectionString = _configuration.ConnectionStrings.ConnectionStrings["MediaCenter"].ConnectionString;
        else if (dataBaseName == "GeoInsights")
            connectionString = _configuration.ConnectionStrings.ConnectionStrings["GeoInsights"].ConnectionString;
        else if (dataBaseName == "Geoservices")
            connectionString = _configuration.ConnectionStrings.ConnectionStrings["Geoservices"].ConnectionString;
        else if (dataBaseName == "GEBIETSASSISTENT")
            connectionString = _configuration.ConnectionStrings.ConnectionStrings["GEBIETSASSISTENT"].ConnectionString;
        else if (dataBaseName == "GEBIETSASSISTENTMonitor")
            connectionString = _configuration.ConnectionStrings.ConnectionStrings["GEBIETSASSISTENT"].ConnectionString;

#if DEBUG
        connectionString = string.Concat(connectionString, ";TrustServerCertificate=true");
#endif

        return connectionString;
    }
}