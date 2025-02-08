using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the repository for interaction with advertisement area geometry data.
/// </summary>
public class AdvertisementAreaGeometryRepository : Repository<AdvertisementAreaGeometry>, IAdvertisementAreaGeometryRepository
{
    public AdvertisementAreaGeometryRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.GeoInsights)
    {
    }

    /// <summary>
    /// It returns a list of advertisement area geometries for a given advertisement area number
    /// </summary>
    /// <param name="advertisementAreaNumber">int</param>
    /// <returns>
    /// A list of AdvertisementAreaGeometry objects.
    /// </returns>
    public List<AdvertisementAreaGeometry> GetAdvertisementAreaGeometryByNumber(int advertisementAreaNumber)
    {
        string query = "";
        if(_environment == "Development")
        {
            query = $@"
                            SELECT 
                                werbegebiets_nr AS {nameof(AdvertisementAreaGeometry.Werbegebiets_nr)}
                                ,medium_id AS {nameof(AdvertisementAreaGeometry.Medium_id)}
                                ,CONVERT(datetime, stand, 4) AS {nameof(AdvertisementAreaGeometry.Stand)}
                                ,CONVERT(datetime, datenstand, 4) AS {nameof(AdvertisementAreaGeometry.Datenstand)}
                                ,anzahl_geoschluessel AS {nameof(AdvertisementAreaGeometry.Anzahl_geoschluessel)}
                                ,CONVERT(datetime, generiert_am, 4) AS {nameof(AdvertisementAreaGeometry.Generiert_am)}
                                ,generiert_in_ms AS {nameof(AdvertisementAreaGeometry.Generiert_in_ms)}
                                ,Geom.STAsBinary() AS {nameof(AdvertisementAreaGeometry.Geom)}
                                ,aktives_gebiet_kunden_id AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_kunden_id)}
                                ,aktives_gebiet_reihenfolge AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_reihenfolge)}
                                ,aktives_gebiet_style_id AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_style_id)}
                                ,hh_brutto AS {nameof(AdvertisementAreaGeometry.HH_brutto)}
                                ,ew AS {nameof(AdvertisementAreaGeometry.Ew)}
                                ,kk_idx AS {nameof(AdvertisementAreaGeometry.KK_idx)}
                                ,berechnungsmethode AS {nameof(AdvertisementAreaGeometry.Berechnungsmethode)}
                                ,geom_is_valid AS {nameof(AdvertisementAreaGeometry.Geom_is_valid)}
	                        FROM dbo.werbegebiete_mit_geometrien
                            WHERE werbegebiets_nr = '{advertisementAreaNumber}';
                            ";
        }
        else if (_environment == "Production")
        {
            query = $@"
                            SELECT 
                                werbegebiets_nr AS {nameof(AdvertisementAreaGeometry.Werbegebiets_nr)}
                                ,medium_id AS {nameof(AdvertisementAreaGeometry.Medium_id)}
                                ,stand AS {nameof(AdvertisementAreaGeometry.Stand)}
                                ,datenstand AS {nameof(AdvertisementAreaGeometry.Datenstand)}
                                ,anzahl_geoschluessel AS {nameof(AdvertisementAreaGeometry.Anzahl_geoschluessel)}
                                ,generiert_am AS {nameof(AdvertisementAreaGeometry.Generiert_am)}
                                ,generiert_in_ms AS {nameof(AdvertisementAreaGeometry.Generiert_in_ms)}
                                ,ST_AsBinary(geom) AS {nameof(AdvertisementAreaGeometry.Geom)}
                                ,aktives_gebiet_kunden_id AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_kunden_id)}
                                ,aktives_gebiet_reihenfolge AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_reihenfolge)}
                                ,aktives_gebiet_style_id AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_style_id)}
                                ,hh_brutto AS {nameof(AdvertisementAreaGeometry.HH_brutto)}
                                ,ew AS {nameof(AdvertisementAreaGeometry.Ew)}
                                ,kk_idx AS {nameof(AdvertisementAreaGeometry.KK_idx)}
                                ,berechnungsmethode AS {nameof(AdvertisementAreaGeometry.Berechnungsmethode)}
                                ,geom_is_valid AS {nameof(AdvertisementAreaGeometry.Geom_is_valid)}
	                        FROM dbo.werbegebiete_mit_geometrien
                            WHERE werbegebiets_nr = '{advertisementAreaNumber}';
                            ";
        }
        

        var result = DbConnection.Query<AdvertisementAreaGeometry>(query).ToList();
        return result;
    }

    /// <summary>
    /// It takes a list of advertisement area numbers and returns a list of advertisement area
    /// geometries
    /// </summary>
    /// <param name="advertisementAreaNumbers">List<int></param>
    /// <returns>
    /// A list of AdvertisementAreaGeometry objects.
    /// </returns>
    public List<AdvertisementAreaGeometry> GetAdvertisementAreaGeometreiysByNumbers(List<int> advertisementAreaNumbers)
    {
        List<AdvertisementAreaGeometry> results = new List<AdvertisementAreaGeometry>();
        foreach (var num in advertisementAreaNumbers)
        {
            string query = "";
            if(_environment == "Development")
            {
                query = $@"
                                SELECT 
                                werbegebiets_nr AS {nameof(AdvertisementAreaGeometry.Werbegebiets_nr)}
                                ,medium_id AS {nameof(AdvertisementAreaGeometry.Medium_id)}
                                ,stand AS {nameof(AdvertisementAreaGeometry.Stand)}
                                ,datenstand AS {nameof(AdvertisementAreaGeometry.Datenstand)}
                                ,anzahl_geoschluessel AS {nameof(AdvertisementAreaGeometry.Anzahl_geoschluessel)}
                                ,generiert_am AS {nameof(AdvertisementAreaGeometry.Generiert_am)}
                                ,generiert_in_ms AS {nameof(AdvertisementAreaGeometry.Generiert_in_ms)}
                                ,geom.STAsBinary() AS {nameof(AdvertisementAreaGeometry.Geom)}
                                ,aktives_gebiet_kunden_id AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_kunden_id)}
                                ,aktives_gebiet_reihenfolge AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_reihenfolge)}
                                ,aktives_gebiet_style_id AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_style_id)}
                                ,hh_brutto AS {nameof(AdvertisementAreaGeometry.HH_brutto)}
                                ,ew AS {nameof(AdvertisementAreaGeometry.Ew)}
                                ,kk_idx AS {nameof(AdvertisementAreaGeometry.KK_idx)}
                                ,berechnungsmethode AS {nameof(AdvertisementAreaGeometry.Berechnungsmethode)}
                                ,geom_is_valid AS {nameof(AdvertisementAreaGeometry.Geom_is_valid)}
	                        FROM dbo.werbegebiete_mit_geometrien
                            WHERE werbegebiets_nr = '{num}';
                                ";
            }
            else if(_environment == "Production")
            {
                query = $@"
                                SELECT 
                                    werbegebiets_nr AS {nameof(AdvertisementAreaGeometry.Werbegebiets_nr)}
                                    ,medium_id AS {nameof(AdvertisementAreaGeometry.Medium_id)}
                                    ,stand AS {nameof(AdvertisementAreaGeometry.Stand)}
                                    ,datenstand AS {nameof(AdvertisementAreaGeometry.Datenstand)}
                                    ,anzahl_geoschluessel AS {nameof(AdvertisementAreaGeometry.Anzahl_geoschluessel)}
                                    ,generiert_am AS {nameof(AdvertisementAreaGeometry.Generiert_am)}
                                    ,generiert_in_ms AS {nameof(AdvertisementAreaGeometry.Generiert_in_ms)}
                                    ,ST_AsBinary(geom) AS {nameof(AdvertisementAreaGeometry.Geom)}
                                    ,aktives_gebiet_kunden_id AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_kunden_id)}
                                    ,aktives_gebiet_reihenfolge AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_reihenfolge)}
                                    ,aktives_gebiet_style_id AS {nameof(AdvertisementAreaGeometry.Aktives_gebiet_style_id)}
                                    ,hh_brutto AS {nameof(AdvertisementAreaGeometry.HH_brutto)}
                                    ,ew AS {nameof(AdvertisementAreaGeometry.Ew)}
                                    ,kk_idx AS {nameof(AdvertisementAreaGeometry.KK_idx)}
                                    ,berechnungsmethode AS {nameof(AdvertisementAreaGeometry.Berechnungsmethode)}
                                    ,geom_is_valid AS {nameof(AdvertisementAreaGeometry.Geom_is_valid)}
	                            FROM public.werbegebiete_mit_geometrien
                                WHERE werbegebiets_nr = '{num}';
                                ";
            } 
                


            var result = DbConnection.Query<AdvertisementAreaGeometry>(query).FirstOrDefault();
            if (result != null)
            {
                results.Add(result);
            }
        }
        if (results.Count > 0)
        {
            return results;
        }

        return new List<AdvertisementAreaGeometry>();
    }
}
