using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Constants;

public struct MapParams
{
    public struct MapManager
    {
        public const string OccupancyUnit = "Ausgabe";
    }
    public struct FeatureClass
    {
        public static Dictionary<string, string> Columns = new Dictionary<string, string>()
            {
                {"Tour_Nr", "string" }, {"Tour", "string" }, { "Auflage", "int" }, { "Auflage_Info", "string" }, { "HH_Brutto", "int" }, {"Erscheintage", "string" },
                { "ID", "int" }, {"Medium_ID", "int" }, {"Medium", "string" }, {"BBE_ID", "int" }, {"Datenquelle", "string"},
                { "Belegungseinheit", "string" }, {"Name_Titel", "string" }, {"Ausgabe_Nr", "string" },
                { "Ausgabe", "string" }, {"Tour_ID", "string" },  {"Datenstand", "date" }, {"generiert_am", "date" },
                { "Anzahl_Geometrien", "int" }, {"Fehlerhafte_Geometrien", "int" }, {"Bereinigt_am", "date" },
                { "Geometrie", "string" }
            };
        public static Dictionary<string, string> AdvertisementArea = new Dictionary<string, string>()
            {
                { "Werbegebiets_nr", "string" }, {"Medium_id", "int" }, {"Stand", "date" }, {"Datenstand", "date" },
                {"Anzahl_geoschluessel", "int" }, {"Generiert_am", "date" }, {"Generiert_in_ms", "int" }, {"Geom", "string" },
                { "Aktives_gebiet_kunden_id", "int" }, {"Aktives_gebiet_reihenfolge", "string" }, {"Aktives_gebiet_style_id", "int" },
                { "HH_brutto", "int" }, {"Ew", "int" }, {"KK_id", "string" }, {"Berechnungsmethode", "string" }, {"Geom_is_valid", "int" }
            };
    }
    public struct ShapeFile
    {
        public static Dictionary<string, string> Columns = new Dictionary<string, string>()
        {
            {"Planungs_Nr", "int"}, {"Planungs_ID", "int"}, {"Filial_Nr", "string" }, {"Geokey", "string"},  {"Geokey_Name", "string"},
            {"HH_Brutto", "int"}, {"HH_Netto", "int"}, {"Geom", "string" }
        };
    }
    public struct FeatureLayers
    {
        public const string Ausgabenebene = "Ausgabenebene";
        public const string PLZ5_Ebene = "PLZ5_Ebene";
        public const string Tour_BBE_Ebene = "Tour_BBE_Ebene";
        public const string MPLZ_dynamisch = "mPLZ_dynamisch";
        public const string PLZ_dynamisch = "PLZ_dynamisch";
        public struct PlanningLevel
        {
            public const string PLZ = "PLZ";
            public const string BBE = "BBE";
        }
    }
    public struct GroupLayers
    {
        public const string Gebietsassistent = "Gebietsassistent";
        public const string Verbreitungsgebiete = "Verbreitungsgebiete";
        public const string Werbegebietsplanung = "Werbegebietsplanung";
        public const string Werbegebiete = "Werbegebiete";
        public const string Datenebenen = "Datenebenen";
    }
    public struct Database
    {
        public const string MPLZ_dynamisch = "GEBIETSASSISTENT.TMP.vw_mPLZ_Selekt";
        public const string PLZ_dynamisch = "GEBIETSASSISTENT.TMP.vw_PLZ_Selekt";
        public const string PLZ_dynamisch1 = "GEBIETSASSISTENT.TMP.vw_PLZ_Selekt1";
        public const string Password = "n8gDjCb3&1!MLdq#gwqN";
    }
}
