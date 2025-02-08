using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the new repository for interaction with geometry data.
/// </summary>
public class GeometryRepositoryGebietsassistent : Repository<Geometry>, IGeometryRepositoryGebietsassistent
{
    private int oldMediaId;
    public GeometryRepositoryGebietsassistent(IDbContext dbContext) : base(dbContext, DbConnectionName.GEBIETSASSISTENT)
    {
    }

    /// <summary>
    /// This function sets the oldMediaId to the id passed in
    /// </summary>
    /// <param name="id">The id of the media item</param>
    private void SetMediaId(int id)
    {
        oldMediaId = id;
    }

    /// <summary>
    /// It executes a stored procedure to get the polygon data from a database and returns it as a
    /// Geometry object.
    /// </summary>
    /// <param name="issueName">The name of the issue, e.g. "Ausgabe 1"</param>
    /// <param name="mediumId">The id of the medium (newspaper, magazine, etc.)</param>
    /// <param name="mediaName">"Bild"</param>
    /// <param name="dataSource">The name of the database</param>
    /// <returns>
    /// A single polygon.
    /// </returns>
    public Geometry GetSinglePolygon(string issueName, int mediumId, string mediaName, string dataSource)
    {
        SetMediaId(mediumId);
        string query = $@"
                            SELECT geom.STAsBinary() AS {nameof(Geometry.Geom)}
                            FROM Verbreitungsgebiete_mit_Geometrie
                            WHERE Medium_ID = '{mediumId}'
                            AND Ausgabe like '{issueName}%'
                            AND Belegungseinheit = 'Ausgabe'
                            ";
        var polygon = DbConnection.QueryFirst<Geometry>(query, commandTimeout: 0);

        if (polygon != null)
        {
            return polygon;
        }
        return null;
    }

    /// <summary>
    /// It executes a stored procedure, sets the mediaId, and then queries the database for a single
    /// polygon
    /// </summary>
    /// <param name="mediumId">The ID of the medium</param>
    /// <param name="mediaName">"Bild"</param>
    /// <param name="dataSource">The name of the database</param>
    /// <returns>
    /// A single polygon.
    /// </returns>
    public Geometry GetSinglePolygon(int mediumId, string mediaName, string dataSource)
    {
        SetMediaId(mediumId);
        string query = $@"
                            SELECT geom.STAsBinary() AS {nameof(Geometry.Geom)}
                            FROM Verbreitungsgebiete_mit_Geometrie
                            WHERE Medium_ID = '{mediumId}'
                            AND Belegungseinheit = 'Ausgabe'
                            ";

        var polygon = DbConnection.QueryFirst<Geometry>(query);

        if (polygon != null)
        {
            return polygon;
        }
        return null;
    }

    /// <summary>
    /// It returns a single geometry object from the database
    /// </summary>
    /// <param name="id">The id of the polygon</param>
    /// <returns>
    /// A Geometry object.
    /// </returns>
    public Geometry GetPolygonById(string id)
    {
        string query = $@"
                            SELECT
                                VG.[ID] AS {nameof(Geometry.Id)}
                                  ,VG.[Medium_ID] AS {nameof(Geometry.MediaId)}
                                  ,VG.[BBE_ID] AS {nameof(Geometry.OccupancyUnitId)}
                                  ,VG.[Datenquelle] AS {nameof(Geometry.DataSource)}
                                  ,VG.[Belegungseinheit] AS {nameof(Geometry.OccupancyUnit)}
                                  ,VG.[Name_Titel] AS {nameof(Geometry.NameTitle)}
                                  ,VG.[Ausgabe_Nr] AS {nameof(Geometry.IssueNumber)}
                                  ,VG.[Ausgabe] AS {nameof(Geometry.Issue)}
                                  ,VG.[Tour_ID] AS {nameof(Geometry.TourId)}
                                  ,VG.[Tour_Nr] AS {nameof(Geometry.TourNumber)}
                                  ,VG.[Tour] AS {nameof(Geometry.TourName)}
                                  ,VG.[Auflage] AS {nameof(Geometry.NumberOfCopies)}
                                  ,VG.[Erscheintage] AS {nameof(Geometry.Appearance)}
                                  ,VG.geom.STNumPoints() AS {nameof(Geometry.GeographyString)}
                                  ,VG.geom.STAsBinary() AS {nameof(Geometry.Geom)}
                                  ,VG.[Datenstand] AS {nameof(Geometry.DataStatus)}
                                  ,VG.[generiert_am] AS {nameof(Geometry.CreationDate)}
                                  ,VG.[Anzahl_Geometrien] AS {nameof(Geometry.NumberOfGeometries)}
                                  ,VG.[Fehlerhafte_Geometrien] AS {nameof(Geometry.NumberOfFaultyGeometries)}
                                  ,VG.[Bereinigt_am] AS {nameof(Geometry.CleaningDate)}
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                                LEFT JOIN dbo.Digitalisierte_BBE BBE
                                    ON VG.BBE_ID = BBE.BBE_ID
                            WHERE VG.ID = {id}
                            AND IsNull(BBE.Archiv, 0) = 0
                            ";
        return DbConnection.Query<Geometry>(query).FirstOrDefault();
    }

    /// <summary>
    /// It executes a stored procedure, sets a media id, creates a list of geometries, loops through
    /// a list of issues, creates a query, and then tries to execute the query. If the query fails,
    /// it continues. If the query succeeds, it adds the result to the list of geometries. If the
    /// list of geometries is empty, it returns an empty list
    /// </summary>
    /// <param name="Media">The media object that contains the id of the media</param>
    /// <param name="issues">A list of issues, e.g. "01/2020", "02/2020", "03/2020"</param>
    /// <param name="mediaName">"BZ"</param>
    /// <param name="occupancyUnit">PLZ, BBE, etc.</param>
    /// <returns>
    /// A list of Geometry objects.
    /// </returns>
    public List<Geometry> GetMultiplePolygons(Media media, List<string> issues, string mediaName, string occupancyUnit)
    {
        ExecuteStoredProcedureDistributionGeography(media.Id, mediaName);
        SetMediaId(media.Id);
        List<Geometry> geometries = new List<Geometry>();
        foreach (var issue in issues)
        {
            string query = $@"
                            SELECT
                                VG.[ID] AS {nameof(Geometry.Id)}
                                  ,VG.[Medium_ID] AS {nameof(Geometry.MediaId)}
                                  ,VG.[BBE_ID] AS {nameof(Geometry.OccupancyUnitId)}
                                  ,VG.[Datenquelle] AS {nameof(Geometry.DataSource)}
                                  ,VG.[Belegungseinheit] AS {nameof(Geometry.OccupancyUnit)}
                                  ,VG.[Name_Titel] AS {nameof(Geometry.NameTitle)}
                                  ,VG.[Ausgabe_Nr] AS {nameof(Geometry.IssueNumber)}
                                  ,VG.[Ausgabe] AS {nameof(Geometry.Issue)}
                                  ,VG.[Tour_ID] AS {nameof(Geometry.TourId)}
                                  ,VG.[Tour_Nr] AS {nameof(Geometry.TourNumber)}
                                  ,VG.[Tour] AS {nameof(Geometry.TourName)}
                                  ,VG.[Auflage] AS {nameof(Geometry.NumberOfCopies)}
                                  ,CASE
                                       WHEN VG.Belegungseinheit = 'PLZ' THEN
                                           G.HH_Brutto
                                       ELSE
                                           BBE.[HH_Brutto]
                                   END AS {nameof(Geometry.GrossHouseHolds)}
                                  ,VG.[Erscheintage] AS {nameof(Geometry.Appearance)}
                                  ,VG.geom.STNumPoints() AS {nameof(Geometry.GeographyString)}
                                  ,VG.geom.STAsBinary() AS {nameof(Geometry.Geom)}
                                  ,VG.[Datenstand] AS {nameof(Geometry.DataStatus)}
                                  ,VG.[generiert_am] AS {nameof(Geometry.CreationDate)}
                                  ,VG.[Anzahl_Geometrien] AS {nameof(Geometry.NumberOfGeometries)}
                                  ,VG.[Fehlerhafte_Geometrien] AS {nameof(Geometry.NumberOfFaultyGeometries)}
                                  ,VG.[Bereinigt_am] AS {nameof(Geometry.CleaningDate)}
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                                LEFT JOIN dbo.Digitalisierte_BBE BBE
                                    ON VG.BBE_ID = BBE.BBE_ID
                                LEFT JOIN dbo.Geoservices_Geometries G
	                                ON G.Geokey = RIGHT(VG.Tour, 5)
                            WHERE VG.Medium_ID = {media.Id}
                            AND VG.Ausgabe like '{issue}%'
                            AND VG.Belegungseinheit = '{occupancyUnit}'
                            AND VG.Datenquelle = '{media.DistributionAreaSource}'
                            AND IsNull(BBE.Archiv, 0) = 0
                            ";

            try
            {
                var result = DbConnection.QueryFirst<Geometry>(query, commandTimeout: 0);
                geometries.Add(result);
            }
            catch (InvalidOperationException)
            {
                continue;
            }


        }
        if (geometries.Count > 0)
        {
            return geometries;
        }
        return new List<Geometry>();
    }

    /// <summary>
    /// It executes a stored procedure, sets a media id, and then queries the database for a list of
    /// geometries
    /// </summary>
    /// <param name="media">This is a class that contains the media id, the media name, and the
    /// distribution area source.</param>
    /// <param name="mediaName">"BZ"</param>
    /// <param name="occupancyUnit">PLZ, BBE, Tour</param>
    /// <returns>
    /// A list of Geometry objects.
    /// </returns>
    public List<Geometry> GetMultiplePolygons(Media media, string mediaName, string occupancyUnit)
    {
        ExecuteStoredProcedureDistributionGeography(media.Id, mediaName);
        SetMediaId(media.Id);
        var archive = occupancyUnit == "BBE" ? 1 : 0;

        string query = $@"
                            SELECT
                                   VG.[ID] AS {nameof(Geometry.Id)}
                                  ,VG.[Medium_ID] AS {nameof(Geometry.MediaId)}
                                  ,M.MEDIUM_NAME AS {nameof(Geometry.MediaName)}
                                  ,VG.[BBE_ID] AS {nameof(Geometry.OccupancyUnitId)}
                                  ,VG.[Datenquelle] AS {nameof(Geometry.DataSource)}
                                  ,VG.[Belegungseinheit] AS {nameof(Geometry.OccupancyUnit)}
                                  ,VG.[Name_Titel] AS {nameof(Geometry.NameTitle)}
                                  ,VG.[Ausgabe_Nr] AS {nameof(Geometry.IssueNumber)}
                                  ,VG.[Ausgabe] AS {nameof(Geometry.Issue)}
                                  ,VG.[Tour_ID] AS {nameof(Geometry.TourId)}
                                  ,VG.[Tour_Nr] AS {nameof(Geometry.TourNumber)}
                                  ,VG.[Tour] AS {nameof(Geometry.TourName)}
                                  ,VG.[Auflage] AS {nameof(Geometry.NumberOfCopies)}
                                  ,VG.[Auflage_Info] AS {nameof(Geometry.NumberOfCopiesInfo)}
                                  ,CASE
                                       WHEN VG.Belegungseinheit = 'PLZ' THEN
                                           G.HH_Brutto
                                       ELSE
                                           BBE.[HH_Brutto]
                                   END AS {nameof(Geometry.GrossHouseHolds)}
                                  ,VG.[Erscheintage] AS {nameof(Geometry.Appearance)}
                                  ,VG.geom.STNumPoints() AS {nameof(Geometry.GeographyString)}
                                  ,VG.geom.STAsBinary() AS {nameof(Geometry.Geom)}
                                  ,VG.[Datenstand] AS {nameof(Geometry.DataStatus)}
                                  ,VG.[generiert_am] AS {nameof(Geometry.CreationDate)}
                                  ,VG.[Anzahl_Geometrien] AS {nameof(Geometry.NumberOfGeometries)}
                                  ,VG.[Fehlerhafte_Geometrien] AS {nameof(Geometry.NumberOfFaultyGeometries)}
                                  ,VG.[Bereinigt_am] AS {nameof(Geometry.CleaningDate)}
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                                LEFT JOIN dbo.Digitalisierte_BBE BBE
                                    ON VG.BBE_ID = BBE.BBE_ID
                                LEFT JOIN dbo.Mediacenter_Medien M
                                    ON VG.Medium_ID = M.MEDIUM_ID
                                LEFT JOIN dbo.Geoservices_Geometries G
	                                ON G.Geokey = RIGHT(VG.Tour, 5)
                            WHERE VG.Medium_ID = {media.Id}
                                  AND VG.Belegungseinheit = '{occupancyUnit}'
                                  AND VG.Datenquelle = '{media.DistributionAreaSource}'
                                  AND IsNull(BBE.Archiv, {archive}) = 0
                        ";



        var results = DbConnection.Query<Geometry>(query, commandTimeout: 0).ToList();
        if (results.Count > 0)
        {
            return results;
        }
        return new List<Geometry>();
    }
    public async Task<List<Geometry>> GetMultiplePolygonsAsync(Media media, string mediaName, string occupancyUnit)
    {
        await ExecuteStoredProcedureDistributionGeographyAsync(media.Id, mediaName);
        SetMediaId(media.Id);
        var archive = occupancyUnit == "BBE" ? 1 : 0;
        string query = $@"
                            SELECT
                                   VG.[ID] AS {nameof(Geometry.Id)}
                                  ,VG.[Medium_ID] AS {nameof(Geometry.MediaId)}
                                  ,M.MEDIUM_NAME AS {nameof(Geometry.MediaName)}
                                  ,VG.[BBE_ID] AS {nameof(Geometry.OccupancyUnitId)}
                                  ,VG.[Datenquelle] AS {nameof(Geometry.DataSource)}
                                  ,VG.[Belegungseinheit] AS {nameof(Geometry.OccupancyUnit)}
                                  ,VG.[Name_Titel] AS {nameof(Geometry.NameTitle)}
                                  ,VG.[Ausgabe_Nr] AS {nameof(Geometry.IssueNumber)}
                                  ,VG.[Ausgabe] AS {nameof(Geometry.Issue)}
                                  ,VG.[Tour_ID] AS {nameof(Geometry.TourId)}
                                  ,VG.[Tour_Nr] AS {nameof(Geometry.TourNumber)}
                                  ,VG.[Tour] AS {nameof(Geometry.TourName)}
                                  ,VG.[Auflage] AS {nameof(Geometry.NumberOfCopies)}
                                  ,VG.[Auflage_Info] AS {nameof(Geometry.NumberOfCopiesInfo)}
                                  ,CASE
                                       WHEN VG.Belegungseinheit = 'PLZ' THEN
                                           G.HH_Brutto
                                       ELSE
                                           BBE.[HH_Brutto]
                                   END AS {nameof(Geometry.GrossHouseHolds)}
                                  ,VG.[Erscheintage] AS {nameof(Geometry.Appearance)}
                                  ,VG.geom.STNumPoints() AS {nameof(Geometry.GeographyString)}
                                  ,VG.geom.STAsBinary() AS {nameof(Geometry.Geom)}
                                  ,VG.[Datenstand] AS {nameof(Geometry.DataStatus)}
                                  ,VG.[generiert_am] AS {nameof(Geometry.CreationDate)}
                                  ,VG.[Anzahl_Geometrien] AS {nameof(Geometry.NumberOfGeometries)}
                                  ,VG.[Fehlerhafte_Geometrien] AS {nameof(Geometry.NumberOfFaultyGeometries)}
                                  ,VG.[Bereinigt_am] AS {nameof(Geometry.CleaningDate)}
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                                LEFT JOIN dbo.Digitalisierte_BBE BBE
                                    ON VG.BBE_ID = BBE.BBE_ID
                                LEFT JOIN dbo.Mediacenter_Medien M
                                    ON VG.Medium_ID = M.MEDIUM_ID
                                LEFT JOIN dbo.Geoservices_Geometries G
	                                ON G.Geokey = RIGHT(VG.Tour, 5)
                            WHERE VG.Medium_ID = {media.Id}
                                  AND VG.Belegungseinheit = '{occupancyUnit}'
                                  AND VG.Datenquelle = '{media.DistributionAreaSource}'
                                  AND IsNull(BBE.Archiv, {archive}) = 0
                        ";
        var results = (await DbConnection.QueryAsync<Geometry>(query, commandTimeout: 0)).ToList();
        if (results.Count > 0)
        {
            return results;
        }
        return new List<Geometry>();
    }
    /// <summary>
    /// It executes a stored procedure, sets a media id, and then queries the database for a list of
    /// geometries
    /// </summary>
    /// <param name="media"></param>
    /// <param name="mediaName">"Wasmuth"</param>
    /// <returns>
    /// A list of Geometry objects.
    /// </returns>
    public List<Geometry> GetPlanningLayerPolygons(Media media, string mediaName)
    {
        ExecuteStoredProcedureDistributionGeography(media.Id, mediaName);
        SetMediaId(media.Id);
        string query = $@"
                            DECLARE @PLZ TABLE
                            (
                                PLZ VARCHAR(5)
                            );

                            INSERT INTO @PLZ
                            (
                                PLZ
                            )
                            SELECT RIGHT(VG.Tour, 5) AS PLZ
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                            WHERE VG.Medium_ID = {media.Id}
                                  AND VG.Belegungseinheit = 'PLZ'
                                  AND VG.Datenquelle = 'Wasmuth'
                                  AND VG.[Auflage_Info] = '1: PLZ genau';

                            SELECT VG.[ID] AS {nameof(Geometry.Id)}
                                  ,VG.[Medium_ID] AS {nameof(Geometry.MediaId)}
                                  ,M.MEDIUM_NAME AS {nameof(Geometry.MediaName)}
                                  ,VG.[BBE_ID] AS {nameof(Geometry.OccupancyUnitId)}
                                  ,VG.[Datenquelle] AS {nameof(Geometry.DataSource)}
                                  ,VG.[Belegungseinheit] AS {nameof(Geometry.OccupancyUnit)}
                                  ,VG.[Name_Titel] AS {nameof(Geometry.NameTitle)}
                                  ,VG.[Ausgabe_Nr] AS {nameof(Geometry.IssueNumber)}
                                  ,VG.[Ausgabe] AS {nameof(Geometry.Issue)}
                                  ,VG.[Tour_ID] AS {nameof(Geometry.TourId)}
                                  ,VG.[Tour_Nr] AS {nameof(Geometry.TourNumber)}
                                  ,VG.[Tour] AS {nameof(Geometry.TourName)}
                                  ,VG.[Auflage] AS {nameof(Geometry.NumberOfCopies)}
                                  ,VG.[Auflage_Info] AS {nameof(Geometry.NumberOfCopiesInfo)}
                                  ,CASE
                                       WHEN VG.Belegungseinheit = 'PLZ' THEN
                                           G.HH_Brutto
                                       ELSE
                                           BBE.[HH_Brutto]
                                   END AS {nameof(Geometry.GrossHouseHolds)}
                                  ,VG.[Erscheintage] AS {nameof(Geometry.Appearance)}
                                  ,VG.geom.STNumPoints() AS {nameof(Geometry.GeographyString)}
                                  ,VG.geom.STAsBinary() AS {nameof(Geometry.Geom)}
                                  ,VG.[Datenstand] AS {nameof(Geometry.DataStatus)}
                                  ,VG.[generiert_am] AS {nameof(Geometry.CreationDate)}
                                  ,VG.[Anzahl_Geometrien] AS {nameof(Geometry.NumberOfGeometries)}
                                  ,VG.[Fehlerhafte_Geometrien] AS {nameof(Geometry.NumberOfFaultyGeometries)}
                                  ,VG.[Bereinigt_am] AS {nameof(Geometry.CleaningDate)}
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                                LEFT JOIN [dbo].[Digitalisierte_BBE_Details] BBED
                                    ON BBED.BBE_ID = VG.BBE_ID
                                LEFT JOIN dbo.Digitalisierte_BBE BBE
                                    ON VG.BBE_ID = BBE.BBE_ID
                                LEFT JOIN dbo.Mediacenter_Medien M
                                    ON VG.Medium_ID = M.MEDIUM_ID
                                LEFT JOIN dbo.Geoservices_Geometries G
	                                ON G.Geokey = RIGHT(VG.Tour, 5)
                           WHERE VG.Medium_ID = {media.Id}
                                  AND VG.Belegungseinheit = 'BBE'
                                  AND ISNULL(BBE.Archiv, 0) = 0
                                  AND VG.Datenquelle = 'Wasmuth'	  
		                          AND NOT EXISTS
                                    (
	                                    SELECT * FROM @PLZ WHERE LEFT(BBED.mPLZ, 5) = PLZ
                                    )
                            GROUP BY VG.geom.STNumPoints(),
                                     VG.geom.STAsBinary(),
                                     VG.ID,
                                     VG.Medium_ID,
                                     M.MEDIUM_NAME,
                                     VG.BBE_ID,
                                     VG.Datenquelle,
                                     VG.Belegungseinheit,
                                     VG.Name_Titel,
                                     VG.Ausgabe_Nr,
                                     VG.Ausgabe,
                                     VG.Tour_ID,
                                     VG.Tour_Nr,
                                     VG.Tour,
                                     VG.Auflage,
                                     VG.Auflage_Info,
                                     CASE
                                       WHEN VG.Belegungseinheit = 'PLZ' THEN
                                           G.HH_Brutto
                                       ELSE
                                           BBE.[HH_Brutto]
									 END,
                                     VG.Erscheintage,
                                     VG.Datenstand,
                                     VG.generiert_am,
                                     VG.Anzahl_Geometrien,
                                     VG.Fehlerhafte_Geometrien,
                                     VG.Bereinigt_am
                            UNION ALL
                            SELECT VG.[ID] AS Id,
                                   VG.[Medium_ID] AS MediaId,
                                   M.MEDIUM_NAME AS MediaName,
                                   VG.[BBE_ID] AS OccupancyUnitId,
                                   VG.[Datenquelle] AS DataSource,
                                   VG.[Belegungseinheit] AS OccupancyUnit,
                                   VG.[Name_Titel] AS NameTitle,
                                   VG.[Ausgabe_Nr] AS IssueNumber,
                                   VG.[Ausgabe] AS Issue,
                                   VG.[Tour_ID] AS TourId,
                                   VG.[Tour_Nr] AS TourNumber,
                                   VG.[Tour] AS TourName,
                                   VG.[Auflage] AS NumberOfCopies,
                                   VG.[Auflage_Info] AS NumberOfCopiesInfo,
                                   VG.[Auflage] AS GrossHouseHolds,
                                   VG.[Erscheintage] AS Appearance,
                                   VG.geom.STNumPoints() AS GeographyString,
                                   VG.geom.STAsBinary() AS Geom,
                                   VG.[Datenstand] AS DataStatus,
                                   VG.[generiert_am] AS CreationDate,
                                   VG.[Anzahl_Geometrien] AS NumberOfGeometries,
                                   VG.[Fehlerhafte_Geometrien] AS NumberOfFaultyGeometries,
                                   VG.[Bereinigt_am] AS CleaningDate
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                                LEFT JOIN dbo.Mediacenter_Medien M
                                    ON VG.Medium_ID = M.MEDIUM_ID
                            WHERE VG.Medium_ID = {media.Id}
                                  AND VG.Belegungseinheit = 'PLZ'
                                  AND VG.Datenquelle = 'Wasmuth'
                                  AND VG.[Auflage_Info] = '1: PLZ genau';
                        ";

        var results = DbConnection.Query<Geometry>(query, commandTimeout: 0).ToList();
        if (results.Count > 0)
        {
            return results;
        }
        return new List<Geometry>();
    }
    public async Task<List<Geometry>> GetPlanningLayerPolygonsAsync(Media media, string mediaName)
    {
        await ExecuteStoredProcedureDistributionGeographyAsync(media.Id, mediaName);
        SetMediaId(media.Id);
        string query = $@"
                            DECLARE @PLZ TABLE
                            (
                                PLZ VARCHAR(5)
                            );

                            INSERT INTO @PLZ
                            (
                                PLZ
                            )
                            SELECT RIGHT(VG.Tour, 5) AS PLZ
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                            WHERE VG.Medium_ID = {media.Id}
                                  AND VG.Belegungseinheit = 'PLZ'
                                  AND VG.Datenquelle = 'Wasmuth'
                                  AND VG.[Auflage_Info] = '1: PLZ genau';

                            SELECT VG.[ID] AS {nameof(Geometry.Id)}
                                  ,VG.[Medium_ID] AS {nameof(Geometry.MediaId)}
                                  ,M.MEDIUM_NAME AS {nameof(Geometry.MediaName)}
                                  ,VG.[BBE_ID] AS {nameof(Geometry.OccupancyUnitId)}
                                  ,VG.[Datenquelle] AS {nameof(Geometry.DataSource)}
                                  ,VG.[Belegungseinheit] AS {nameof(Geometry.OccupancyUnit)}
                                  ,VG.[Name_Titel] AS {nameof(Geometry.NameTitle)}
                                  ,VG.[Ausgabe_Nr] AS {nameof(Geometry.IssueNumber)}
                                  ,VG.[Ausgabe] AS {nameof(Geometry.Issue)}
                                  ,VG.[Tour_ID] AS {nameof(Geometry.TourId)}
                                  ,VG.[Tour_Nr] AS {nameof(Geometry.TourNumber)}
                                  ,VG.[Tour] AS {nameof(Geometry.TourName)}
                                  ,VG.[Auflage] AS {nameof(Geometry.NumberOfCopies)}
                                  ,VG.[Auflage_Info] AS {nameof(Geometry.NumberOfCopiesInfo)}
                                  ,CASE
                                       WHEN VG.Belegungseinheit = 'PLZ' THEN
                                           G.HH_Brutto
                                       ELSE
                                           BBE.[HH_Brutto]
                                   END AS {nameof(Geometry.GrossHouseHolds)}
                                  ,VG.[Erscheintage] AS {nameof(Geometry.Appearance)}
                                  ,VG.geom.STNumPoints() AS {nameof(Geometry.GeographyString)}
                                  ,VG.geom.STAsBinary() AS {nameof(Geometry.Geom)}
                                  ,VG.[Datenstand] AS {nameof(Geometry.DataStatus)}
                                  ,VG.[generiert_am] AS {nameof(Geometry.CreationDate)}
                                  ,VG.[Anzahl_Geometrien] AS {nameof(Geometry.NumberOfGeometries)}
                                  ,VG.[Fehlerhafte_Geometrien] AS {nameof(Geometry.NumberOfFaultyGeometries)}
                                  ,VG.[Bereinigt_am] AS {nameof(Geometry.CleaningDate)}
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                                LEFT JOIN [dbo].[Digitalisierte_BBE_Details] BBED
                                    ON BBED.BBE_ID = VG.BBE_ID
                                LEFT JOIN dbo.Digitalisierte_BBE BBE
                                    ON VG.BBE_ID = BBE.BBE_ID
                                LEFT JOIN dbo.Mediacenter_Medien M
                                    ON VG.Medium_ID = M.MEDIUM_ID
                                LEFT JOIN dbo.Geoservices_Geometries G
	                                ON G.Geokey = RIGHT(VG.Tour, 5)
                           WHERE VG.Medium_ID = {media.Id}
                                  AND VG.Belegungseinheit = 'BBE'
                                  AND ISNULL(BBE.Archiv, 0) = 0
                                  AND VG.Datenquelle = 'Wasmuth'	  
		                          AND NOT EXISTS
                                    (
	                                    SELECT * FROM @PLZ WHERE LEFT(BBED.mPLZ, 5) = PLZ
                                    )
                            GROUP BY VG.geom.STNumPoints(),
                                     VG.geom.STAsBinary(),
                                     VG.ID,
                                     VG.Medium_ID,
                                     M.MEDIUM_NAME,
                                     VG.BBE_ID,
                                     VG.Datenquelle,
                                     VG.Belegungseinheit,
                                     VG.Name_Titel,
                                     VG.Ausgabe_Nr,
                                     VG.Ausgabe,
                                     VG.Tour_ID,
                                     VG.Tour_Nr,
                                     VG.Tour,
                                     VG.Auflage,
                                     VG.Auflage_Info,
                                     CASE
                                       WHEN VG.Belegungseinheit = 'PLZ' THEN
                                           G.HH_Brutto
                                       ELSE
                                           BBE.[HH_Brutto]
									 END,
                                     VG.Erscheintage,
                                     VG.Datenstand,
                                     VG.generiert_am,
                                     VG.Anzahl_Geometrien,
                                     VG.Fehlerhafte_Geometrien,
                                     VG.Bereinigt_am
                            UNION ALL
                            SELECT VG.[ID] AS Id,
                                   VG.[Medium_ID] AS MediaId,
                                   M.MEDIUM_NAME AS MediaName,
                                   VG.[BBE_ID] AS OccupancyUnitId,
                                   VG.[Datenquelle] AS DataSource,
                                   VG.[Belegungseinheit] AS OccupancyUnit,
                                   VG.[Name_Titel] AS NameTitle,
                                   VG.[Ausgabe_Nr] AS IssueNumber,
                                   VG.[Ausgabe] AS Issue,
                                   VG.[Tour_ID] AS TourId,
                                   VG.[Tour_Nr] AS TourNumber,
                                   VG.[Tour] AS TourName,
                                   VG.[Auflage] AS NumberOfCopies,
                                   VG.[Auflage_Info] AS NumberOfCopiesInfo,
                                   VG.[Auflage] AS GrossHouseHolds,
                                   VG.[Erscheintage] AS Appearance,
                                   VG.geom.STNumPoints() AS GeographyString,
                                   VG.geom.STAsBinary() AS Geom,
                                   VG.[Datenstand] AS DataStatus,
                                   VG.[generiert_am] AS CreationDate,
                                   VG.[Anzahl_Geometrien] AS NumberOfGeometries,
                                   VG.[Fehlerhafte_Geometrien] AS NumberOfFaultyGeometries,
                                   VG.[Bereinigt_am] AS CleaningDate
                            FROM Verbreitungsgebiete_mit_Geometrie VG
                                LEFT JOIN dbo.Mediacenter_Medien M
                                    ON VG.Medium_ID = M.MEDIUM_ID
                            WHERE VG.Medium_ID = {media.Id}
                                  AND VG.Belegungseinheit = 'PLZ'
                                  AND VG.Datenquelle = 'Wasmuth'
                                  AND VG.[Auflage_Info] = '1: PLZ genau';
                        ";

        var results = (await DbConnection.QueryAsync<Geometry>(query, commandTimeout: 0)).ToList();
        if (results.Count > 0)
        {
            return results;
        }
        return new List<Geometry>();
    }

    /// <summary>
    /// It executes a stored procedure in a SQL Server database
    /// </summary>
    /// <param name="mediumId">int</param>
    /// <param name="mediaName">The name of the medium (e.g. "Bild")</param>
    /// <param name="removeArtefacts">0 or 1</param>
    public void ExecuteStoredProcedureDistributionGeography(int mediumId, string mediaName, int removeArtefacts = 0)
    {
        if (oldMediaId == mediumId)
        {
            return;
        }
        try
        {
            var procedure = "Prepare_Distribution_Geography";
            var values = new
            {
                Medium_ID = mediumId,
                PLZ_Grenzartefakte_entfernen = removeArtefacts
            };

            DbConnection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            MessageBox.Show($@"Das Gebiet {mediaName} konnte nicht geladen werden. {ex.Message}",
                "Ladefehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message,
                "Ladefehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public async Task ExecuteStoredProcedureDistributionGeographyAsync(int mediumId, string mediaName, int removeArtefacts = 0)
    {
        if (oldMediaId == mediumId)
        {
            return;
        }
        try
        {
            var procedure = "Prepare_Distribution_Geography";
            var values = new
            {
                Medium_ID = mediumId,
                PLZ_Grenzartefakte_entfernen = removeArtefacts
            };

            await DbConnection.ExecuteAsync(procedure, values, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            MessageBox.Show($@"Das Gebiet {mediaName} konnte nicht geladen werden. {ex.Message}",
                "Ladefehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message,
                "Ladefehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>
    /// It executes a stored procedure in the database
    /// </summary>
    /// <param name="mediumId">int</param>
    /// <param name="issue">string</param>
    /// <param name="provideNeighborZipCodes">0 or 1</param>
    public void ExecuteStoredProcedureDistributionSelectionTable(int mediumId, string issue, int provideNeighborZipCodes = 0)
    {
        var procedure = "Prepare_Distribution_Selection_Table";
        var values = new { Medium_ID = mediumId, Ausgabe = issue, Benutzername = $@"{Environment.UserDomainName}\{Environment.UserName.ToLower()}", Angrenzende_PLZ_bereitstellen = provideNeighborZipCodes };
        DbConnection.Execute(procedure, values, null, 1000, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// It returns a list of tours for a given media id
    /// </summary>
    /// <param name="id">the id of the media</param>
    /// <param name="dataSource">"BBE"</param>
    /// <param name="onlyNotDigitized">If true, only tours that have not been digitized will be
    /// returned.</param>
    /// <returns>
    /// A list of Tour objects.
    /// </returns>
    public List<Tour> GetToursList(string id, string dataSource, bool onlyNotDigitized)
    {
        string notDigitizedSnippet = "";
        if (onlyNotDigitized)
        {
            notDigitizedSnippet += "AND Det.BBE_ID is null";
        }

        if (id != null)
        {
            string query = $@"
                            SELECT
                            BBE.[BBE_ID] AS {nameof(Tour.OccupancyUnitId)},
                            BBE.[Medium_ID] AS {nameof(Tour.MediaId)},
                            BBE.[Ausgabe] AS {nameof(Tour.Issue)},
                            BBE.Tour as {nameof(Tour.TourName)},
                            BBE.Tour_ID as {nameof(Tour.TourId)},
                            BBE.Tour_Nr AS {nameof(Tour.TourNumber)},
                            BBE.Auflage AS {nameof(Tour.PrintNumber)},
                            BBE.Digitalisierung_von AS {nameof(Tour.Author)}
                            FROM dbo.Digitalisierte_BBE BBE
                            LEFT JOIN dbo.Digitalisierte_BBE_Details Det
                            ON BBE.BBE_ID = Det.BBE_ID
                            WHERE [Medium_ID] = {id}
                            AND BBE.Archiv = 0
                            AND Gesplittet = 0
                            AND Datenquelle = '{dataSource}'
                            {notDigitizedSnippet}
                            GROUP BY bbe.Tour_Nr,
                            BBE.[BBE_ID],
                            BBE.[Medium_ID],
                            BBE.[Ausgabe],
                            BBE.Tour,
                            BBE.Tour_ID,
                            BBE.Tour_Nr,
                            BBE.Auflage,
                            BBE.Digitalisierung_von,
                            BBE.Grenzstand,
                            BBE.Datenstand,
                            Bbe.Digitalisierung_begonnen
                            ";

            var result = DbConnection.Query<Tour>(query).ToList();
            return result;
        }
        return new List<Tour>();
    }

    /// <summary>
    /// It returns a list of occupancy units for a given occupancy unit id
    /// </summary>
    /// <param name="occupancyUnitId">int</param>
    /// <returns>
    /// A list of OccupancyUnit objects.
    /// </returns>
    public List<OccupancyUnit> GetDigitizedOccupancyUnitsList(int occupancyUnitId)
    {
        string query = $@"
                            SELECT 
                                BBE.Medium_ID,
                                BBE.Tour,
                                BBE.Geprüft_von AS {nameof(OccupancyUnit.EvaluatedFrom)},
                                BBE.Geprüft_am AS {nameof(OccupancyUnit.EvaluatedOn)},
                                BBED.BBE_ID,
                                BBED.digitalisiert_am AS {nameof(OccupancyUnit.LastModified)},
                                BBED.digitalisiert_von AS {nameof(OccupancyUnit.Author)},
                                BBED.Stand AS {nameof(OccupancyUnit.BorderStatus)},
                                HH.PLZ AS {nameof(OccupancyUnit.ZipCode)},
                                HH.mPLZ AS {nameof(OccupancyUnit.MicroZipCode)},
                                HH.PLZ_NAME AS {nameof(OccupancyUnit.ZipCodeName)},
                                HH.Ortsteile AS {nameof(OccupancyUnit.Districts)},
                                HH.Ortsteil_Anteile_HH AS {nameof(OccupancyUnit.DistrictHousholdsProportion)},
                                HH.KGS8 AS {nameof(OccupancyUnit.CommutityCode)},
                                HH.KGS8_NAME AS {nameof(OccupancyUnit.CommunityName)},
                                HH.EW AS {nameof(OccupancyUnit.Residents)},
                                HH.HH_BRUTTO AS {nameof(OccupancyUnit.GrossHousehold)},
                                HH.HH_NETTO AS {nameof(OccupancyUnit.NetHouesehold)},
                                ROUND(HH.WV_QUOTE, 2) * 100 AS {nameof(OccupancyUnit.AdvertisingObjectorsQuote)}
                            from dbo.Geodaten_HAUSHALTE_MPLZ HH
                            Inner Join [dbo].[Digitalisierte_BBE_Details] BBED 
                                ON BBED.mPLZ = HH.mPLZ
                            Inner Join [dbo].[Digitalisierte_BBE] BBE
                                ON BBE.BBE_ID = BBED.BBE_ID
                            WHERE BBE.BBE_Id = {occupancyUnitId}
                            ";

        return DbConnection.Query<OccupancyUnit>(query).ToList();
    }

    /// <summary>
    /// It returns a list of tours for a given occupancy unit id
    /// </summary>
    /// <param name="occupancyUnitId">This is the ID of the occupancy unit.</param>
    /// <returns>
    /// A list of Tour objects.
    /// </returns>
    public List<Tour> GetAreasList(string occupancyUnitId)
    {
        if (occupancyUnitId != null)
        {
            string query = $@"
                            SELECT DISTINCT
                            ID as {nameof(Tour.Id)},
                            Medium_ID AS {nameof(Tour.MediaId)}, 
                            [Ausgabenname (Wasmuth)] AS {nameof(Tour.Issue)},
                            Tour as {nameof(Tour.TourName)}, 
                            Tour_ID as {nameof(Tour.TourId)}, 
                            Tour_Nr AS {nameof(Tour.TourNumber)}, 
                            PLZ AS {nameof(Tour.ZipCode)},
                            Ort AS {nameof(Tour.Location)},
                            Ortsteil AS {nameof(Tour.District)},
                            Auflage AS {nameof(Tour.PrintNumber)}
                            FROM dbo.Digitalisierte_BBE_Gebietslisten
                            WHERE BBE_Id = {occupancyUnitId}
                            ";

            var result = DbConnection.Query<Tour>(query).ToList();
            return result;
        }
        return new List<Tour>();
    }

    public List<Tour> GetAreasList(string occupancyUnitId, SqlTransaction transaction)
    {
        if (occupancyUnitId != null)
        {
            string query = $@"
                            SELECT DISTINCT
                            ID as {nameof(Tour.Id)},
                            Medium_ID AS {nameof(Tour.MediaId)}, 
                            [Ausgabenname (Wasmuth)] AS {nameof(Tour.Issue)},
                            Tour as {nameof(Tour.TourName)}, 
                            Tour_ID as {nameof(Tour.TourId)}, 
                            Tour_Nr AS {nameof(Tour.TourNumber)}, 
                            PLZ AS {nameof(Tour.ZipCode)},
                            Ort AS {nameof(Tour.Location)},
                            Ortsteil AS {nameof(Tour.District)},
                            Auflage AS {nameof(Tour.PrintNumber)}
                            FROM dbo.Digitalisierte_BBE_Gebietslisten
                            WHERE BBE_Id = {occupancyUnitId}
                            ";

            var sqlCommand = new SqlCommand(query, transaction.Connection, transaction);
            var reader = sqlCommand.ExecuteReader();

            var result = new List<Tour>();

            while (reader.Read())
            {
                int printNumber = 0;
                if (int.TryParse(reader[nameof(Tour.PrintNumber)].ToString(), out int pnumber))
                    printNumber = pnumber;
                Tour tour = new Tour();
                tour.Id = int.Parse(reader[nameof(Tour.Id)].ToString());
                tour.MediaId = reader[nameof(Tour.MediaId)].ToString();
                tour.Issue = reader[nameof(Tour.Issue)].ToString();
                tour.TourName = reader[nameof(Tour.TourName)].ToString();
                tour.TourId = reader[nameof(Tour.TourId)].ToString();
                tour.TourNumber = reader[nameof(Tour.TourNumber)].ToString();
                tour.ZipCode = reader[nameof(Tour.ZipCode)].ToString();
                tour.Location = reader[nameof(Tour.Location)].ToString();
                tour.District = reader[nameof(Tour.District)].ToString();
                tour.PrintNumber = printNumber;
                result.Add(tour);
            }
            reader.Close();
            return result;
        }
        return new List<Tour>();
    }

    /// <summary>
    /// It returns a list of tours that are near to a list of micro zip codes
    /// </summary>
    /// <param name="microZipCodes">A list of micro zip codes (mPLZ)</param>
    /// <returns>
    /// A list of tours.
    /// </returns>
    public List<Tour> GetNearToursOfMissingUnits(List<string> microZipCodes)
    {
        List<Tour> results = new List<Tour>();
        foreach (var microZipCode in microZipCodes)
        {
            string query = $@"
                            SELECT DISTINCT
                            BBE.[BBE_ID] AS {nameof(Tour.OccupancyUnitId)},
                            BBE.[Medium_ID] AS {nameof(Tour.MediaId)}, 
                            BBE.[Ausgabe] AS {nameof(Tour.Issue)},
                            BBE.Tour as {nameof(Tour.TourName)}, 
                            BBE.Tour_ID as {nameof(Tour.TourId)}, 
                            BBE.Tour_Nr AS {nameof(Tour.TourNumber)},
                            BBE.Auflage AS {nameof(Tour.PrintNumber)},
                            BBE.Digitalisierung_von AS {nameof(Tour.Author)}
                            --,NB.Entfernung As {nameof(Tour.DistanceToOccupancyUnit)}
                            FROM dbo.Digitalisierte_BBE BBE
                            LEFT JOIN dbo.Digitalisierte_BBE_Details Det
                            ON BBE.BBE_ID = Det.BBE_ID
                            Inner Join [TMP].[vw_nahegelegene_BBE_fehlender_mPLZ_genauer] NB 
                            On NB.BBE_ID = BBE.BBE_ID
                            Where NB.mPLZ = '{microZipCode}'
                            AND NB.Entfernung = 0
                            ";

            var result = DbConnection.Query<Tour>(query).ToList();
            if (result != null)
            {
                results.AddRange(result);
            }
        }
        return results;
    }

    /// <summary>
    /// It returns a list of tours that have been updated and changed
    /// </summary>
    /// <returns>
    /// A list of Tour objects.
    /// </returns>
    public List<Tour> GetUpdatedAndChangedTours()
    {
        string query = $@"
                        SELECT BBE.BBE_ID AS {nameof(Tour.OccupancyUnitId)}
                                ,BBE.Medium_ID AS {nameof(Tour.MediaId)}
                                ,BBE.[Ausgabe] AS {nameof(Tour.Issue)}
        	                    ,BBE.Tour AS {nameof(Tour.TourName)}
                                ,BBE.[Tour_ID] AS {nameof(Tour.TourId)}
                                ,BBE.[Tour_Nr] AS {nameof(Tour.TourNumber)}
                        FROM dbo.Digitalisierte_BBE BBE
                            INNER JOIN
                            (
                                SELECT Neue_BBE_ID
                                FROM dbo.Digitalisierte_BBE
                                WHERE Neue_BBE_ID IS NOT NULL
                                        AND Archiv = 1
                                        AND Digitalisierung_von IS NOT NULL
                            ) Alt
                                ON Alt.Neue_BBE_ID = BBE.BBE_ID
                        WHERE BBE.Archiv = 0
                                AND BBE.Digitalisierung_von IS NULL
                        ";

        var result = DbConnection.Query<Tour>(query).ToList();
        if (result != null)
        {
            return result;
        }
        return new List<Tour>(); ;
    }

    public async Task<List<Tour>> GetUpdatedAndChangedToursAsync()
    {
        string query = $@"
                        SELECT BBE.BBE_ID AS {nameof(Tour.OccupancyUnitId)}
                                ,BBE.Medium_ID AS {nameof(Tour.MediaId)}
                                ,BBE.[Ausgabe] AS {nameof(Tour.Issue)}
        	                    ,BBE.Tour AS {nameof(Tour.TourName)}
                                ,BBE.[Tour_ID] AS {nameof(Tour.TourId)}
                                ,BBE.[Tour_Nr] AS {nameof(Tour.TourNumber)}
                        FROM dbo.Digitalisierte_BBE BBE
                            INNER JOIN
                            (
                                SELECT Neue_BBE_ID
                                FROM dbo.Digitalisierte_BBE
                                WHERE Neue_BBE_ID IS NOT NULL
                                        AND Archiv = 1
                                        AND Digitalisierung_von IS NOT NULL
                            ) Alt
                                ON Alt.Neue_BBE_ID = BBE.BBE_ID
                        WHERE BBE.Archiv = 0
                                AND BBE.Digitalisierung_von IS NULL
                        ";

        var result = (await DbConnection.QueryAsync<Tour>(query)).ToList();
        if (result != null)
        {
            return result;
        }
        return new List<Tour>(); ;
    }

    /// <summary>
    /// It returns a list of occupancy units for a given zip code
    /// </summary>
    /// <param name="zipCode">string</param>
    /// <returns>
    /// A list of OccupancyUnit objects.
    /// </returns>
    public List<OccupancyUnit> GetOccupancyUnits(string zipCode)
    {
        if (zipCode != null)
        {
            string query = $@"
                            select 
                            PLZ AS {nameof(OccupancyUnit.ZipCode)},
                            mPLZ AS {nameof(OccupancyUnit.MicroZipCode)},
                            PLZ_NAME AS {nameof(OccupancyUnit.ZipCodeName)},
                            Ortsteile As {nameof(OccupancyUnit.Districts)},
                            Ortsteil_Anteile_HH AS {nameof(OccupancyUnit.DistrictHousholdsProportion)},
                            KGS8 AS {nameof(OccupancyUnit.CommutityCode)},
                            KGS8_NAME AS {nameof(OccupancyUnit.CommunityName)},
                            EW AS {nameof(OccupancyUnit.Residents)},
                            HH_BRUTTO AS {nameof(OccupancyUnit.GrossHousehold)},
                            HH_NETTO AS {nameof(OccupancyUnit.NetHouesehold)},
                            ROUND(WV_QUOTE, 2) * 100 AS {nameof(OccupancyUnit.AdvertisingObjectorsQuote)},
                            STAND AS {nameof(OccupancyUnit.BorderStatus)}
                            from dbo.Geodaten_HAUSHALTE_MPLZ
                            Where PLZ = '{zipCode}'
                            ";

            var result = DbConnection.Query<OccupancyUnit>(query).ToList();
            return result;
        }
        return new List<OccupancyUnit>();
    }

    /// <summary>
    /// It returns a list of OccupancyUnits from a database table
    /// </summary>
    /// <returns>
    /// A list of OccupancyUnit objects.
    /// </returns>
    public List<OccupancyUnit> GetMissingOccupancyUnits()
    {
        string query = $@"
                        select 
                        HH.PLZ AS {nameof(OccupancyUnit.ZipCode)},
                        HH.mPLZ AS {nameof(OccupancyUnit.MicroZipCode)},
                        HH.PLZ_NAME AS {nameof(OccupancyUnit.ZipCodeName)},
                        HH.Ortsteile As {nameof(OccupancyUnit.Districts)},
                        HH.Ortsteil_Anteile_HH AS {nameof(OccupancyUnit.DistrictHousholdsProportion)},
                        HH.KGS8 AS {nameof(OccupancyUnit.CommutityCode)},
                        HH.KGS8_NAME AS {nameof(OccupancyUnit.CommunityName)},
                        HH.EW AS {nameof(OccupancyUnit.Residents)},
                        HH.HH_BRUTTO AS {nameof(OccupancyUnit.GrossHousehold)},
                        HH.HH_NETTO AS {nameof(OccupancyUnit.NetHouesehold)},
                        ROUND(HH.WV_QUOTE, 2) * 100 AS {nameof(OccupancyUnit.AdvertisingObjectorsQuote)},
                        HH.STAND AS {nameof(OccupancyUnit.BorderStatus)}
                        FROM dbo.Geodaten_HAUSHALTE_MPLZ HH
                        INNER JOIN TMP.vw_fehlende_mPLZ F ON HH.mPLZ = F.mPLZ
                        ";

        var result = DbConnection.Query<OccupancyUnit>(query).ToList();
        if (result != null)
        {
            return result;
        }
        return new List<OccupancyUnit>();
    }

    /// <summary>
    /// > This function takes a delimiter and a list of integers and returns a string of the list
    /// items separated by the delimiter
    /// </summary>
    /// <param name="delimiter">The delimiter to use when joining the list items.</param>
    /// <param name="items">The list of items to join</param>
    /// <returns>
    /// A string
    /// </returns>
    private string JoinListItems(string delimiter, List<int> items)
    {
        string result = "";
        int counter = 0;
        foreach (var item in items)
        {
            result += counter == 0 ? item.ToString() : delimiter + item.ToString();
            counter++;
        }
        return result;
    }

    /// <summary>
    /// It returns the latest version of a list of occupancy units
    /// </summary>
    /// <param name="occupancyUnitIds">List of occupancy unit ids</param>
    /// <returns>
    /// A DateTime object.
    /// </returns>
    public DateTime GetLatestVersionOfOccupancyUnits(List<int> occupancyUnitIds)
    {
        string occupancyUnitIdList = JoinListItems(",", occupancyUnitIds);

        string query = $@"
                            SELECT MAX(Stand)
                            FROM dbo.Digitalisierte_BBE_Details
                            WHERE BBE_ID IN ({occupancyUnitIdList});
                            ";

        return DbConnection.ExecuteScalar<DateTime>(query);
    }

    /// <summary>
    /// It returns the latest version of the zip codes
    /// </summary>
    /// <returns>
    /// A DateTime object
    /// </returns>
    public DateTime GetLatestVersionOfZipCodes()
    {
        string query = $@"
                            SELECT Letzte_Änderung FROM [dbo].[Geoservices_Geometries_LatestVersion]
                            ";

        return DbConnection.ExecuteScalar<DateTime>(query);
    }

    /// <summary>
    /// It takes a tour name and a list of sub tours and inserts them into the database
    /// </summary>
    /// <param name="occupancyUnitId">The id of the row in the table that is being split</param>
    /// <param name="tourName">The name of the tour that is to be split</param>
    /// <param name="subTours">List of Tour objects</param>
    public void InsertSubTours(int? occupancyUnitId, string tourName, List<Tour> subTours)
    {
        UpdateTourSplitState(occupancyUnitId, 1);
        foreach (var tour in subTours)
        {
            string sql = $@"
                              INSERT INTO dbo.Digitalisierte_BBE
                              (
                                  Medium_Ausgabe_ID,
                                  Medium_ID,
                                  Datenquelle,
                                  Medienname,
                                  Ausgabe_Nr,
                                  Ausgabe,
                                  Tour_ID,
                                  Tour_Nr,
                                  Tour,
                                  Auflage,
                                  Datenstand,
                                  Grenzstand,
                                  Digitalisierung_Status_ID,
                                  Digitalisierung_von,
                                  Auto_Digitalisierung_Komplexität,
                                  Auto_Digitalisierung_Score,
                                  Digitalisierung_begonnen,
                                  Digitalisierung_letzte_Bearbeitung,
                                  Digitalisierung_beendet,
                                  Digitalisierung_Dauer,
                                  Aggregation_durchgeführt_am,
                                  Archiv,
                                  Neue_BBE_ID,
                                  Status_Gebietslistenvergleich,
                                  Gesplittet,
                                  Stammtour
                              )
                              SELECT Medium_Ausgabe_ID,
                                  Medium_ID,
                                  Datenquelle,
                                  Medienname,
                                  Ausgabe_Nr,
                                  Ausgabe,
                                  Tour_ID,
                                  Tour_Nr,
                                  '{tour.TourName}',
                                  Auflage,
                                  Datenstand,
                                  Grenzstand,
                                  Digitalisierung_Status_ID,
                                  Digitalisierung_von,
                                  Auto_Digitalisierung_Komplexität,
                                  Auto_Digitalisierung_Score,
                                  Digitalisierung_begonnen,
                                  Digitalisierung_letzte_Bearbeitung,
                                  Digitalisierung_beendet,
                                  Digitalisierung_Dauer,
                                  Aggregation_durchgeführt_am,
                                  Archiv,
                                  Neue_BBE_ID,
                                  Status_Gebietslistenvergleich,
                                  0,
                                  '{tourName}'
	                              FROM dbo.Digitalisierte_BBE
                              WHERE BBE_ID = {occupancyUnitId}
                              AND Tour = '{tourName}'                        
                            ";

            DbConnection.Execute(sql);
        }
    }

    public void InsertSubTours(int? occupancyUnitId, string tourName, List<Tour> subTours, SqlTransaction transaction)
    {
        UpdateTourSplitState(occupancyUnitId, 1, transaction);
        foreach (var tour in subTours)
        {
            string sql = $@"
                              INSERT INTO dbo.Digitalisierte_BBE
                              (
                                  Medium_Ausgabe_ID,
                                  Medium_ID,
                                  Medium_ID_WM,
                                  Datenquelle,
                                  Medienname,
                                  Ausgabe_Nr,
                                  Ausgabe,
                                  Tour_ID,
                                  Tour_Nr,
                                  Tour,
                                  Auflage,
                                  Datenstand,
                                  Grenzstand,
                                  Digitalisierung_Status_ID,
                                  Digitalisierung_von,
                                  Auto_Digitalisierung_Komplexität,
                                  Auto_Digitalisierung_Score,
                                  Digitalisierung_begonnen,
                                  Digitalisierung_letzte_Bearbeitung,
                                  Digitalisierung_beendet,
                                  Digitalisierung_Dauer,
                                  Aggregation_durchgeführt_am,
                                  Archiv,
                                  Neue_BBE_ID,
                                  Status_Gebietslistenvergleich,
                                  Gesplittet,
                                  Stammtour
                              )
                              SELECT Medium_Ausgabe_ID,
                                  Medium_ID,
                                  Medium_ID_WM,
                                  Datenquelle,
                                  Medienname,
                                  Ausgabe_Nr,
                                  Ausgabe,
                                  Tour_ID,
                                  Tour_Nr,
                                  '{tour.TourName}',
                                  {tour.PrintNumber},
                                  Datenstand,
                                  Grenzstand,
                                  Digitalisierung_Status_ID,
                                  Digitalisierung_von,
                                  Auto_Digitalisierung_Komplexität,
                                  Auto_Digitalisierung_Score,
                                  Digitalisierung_begonnen,
                                  Digitalisierung_letzte_Bearbeitung,
                                  Digitalisierung_beendet,
                                  Digitalisierung_Dauer,
                                  Aggregation_durchgeführt_am,
                                  Archiv,
                                  Neue_BBE_ID,
                                  Status_Gebietslistenvergleich,
                                  0,
                                  '{tourName}'
	                              FROM dbo.Digitalisierte_BBE
                              WHERE BBE_ID = {occupancyUnitId}
                              AND Tour = '{tourName}'                        
                            ";

            var sqlCommand = new SqlCommand(sql, transaction.Connection, transaction);
            sqlCommand.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// It updates the database with the new split state of the occupancy unit
    /// </summary>
    /// <param name="occupancyUnitId">The id of the occupancy unit</param>
    /// <param name="splittState">0 or 1</param>
    private void UpdateTourSplitState(int? occupancyUnitId, int splittState)
    {
        string sql = $@"
                            UPDATE dbo.Digitalisierte_BBE
                            SET Gesplittet = {splittState}
                            WHERE BBE_ID = {occupancyUnitId}
                            ";

        DbConnection.Execute(sql);
    }
    private void UpdateTourSplitState(int? occupancyUnitId, int splittState, SqlTransaction transaction)
    {
        string sql = $@"
                            UPDATE dbo.Digitalisierte_BBE
                            SET Gesplittet = {splittState}
                            WHERE BBE_ID = {occupancyUnitId}
                            ";

        var sqlCommand = new SqlCommand(sql, transaction.Connection, transaction);
        sqlCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// It inserts new rows into a table, based on the values of the rows that are already in the
    /// table
    /// </summary>
    /// <param name="occupancyUnitId">The id of the occupancy unit that is being split</param>
    /// <param name="tourName">The name of the tour that is being split</param>
    /// <param name="subTours">List of Tour objects</param>

    public void InsertAreasWithNewTour(int? occupancyUnitId, string tourName, List<Tour> subTours, SqlTransaction transaction)
    {
        UpdateAreaSplitState(tourName, 1, transaction);
        var mediaId = subTours.FirstOrDefault().MediaId;
        var insertedSubTours = GetInsertedSubTours(mediaId, tourName, transaction);
        var areas = GetAreasList(occupancyUnitId.ToString(), transaction);
        
        var subTourIds = new List<int?>();

        foreach (var area in areas)
        {
            var tour = insertedSubTours.Where(t => t.Id == area.Id).FirstOrDefault();
            var subTour = subTours.Where(t => t.TourName == tour.TourName).FirstOrDefault();
            var printNumber = subTourIds.Contains(tour.OccupancyUnitId) ? 0 : subTour.PrintNumber;

            string sql = $@"
                            INSERT INTO dbo.Digitalisierte_BBE_Gebietslisten
                            (
                                BBE_ID,
                                Medium_ID,
                                [Medium-ID (WM)],
                                [Medienname (Wasmuth)],
                                Erscheintage,
                                WoAnf,
                                WoMi,
                                WoEnd,
                                Tarif_id,
                                Tarif_Nr,
                                Tarif_Name,
                                gültig_ab,
                                gültig_bis,
                                [Ausgabenname (Wasmuth)],
                                [Ausgaben-Nr],
                                Tour_ID,
                                Tour_Nr,
                                Tour,
                                PLZ,
                                Ort,
                                Ortsteil,
                                Auflage,
                                [Auflage Info],
                                [HH der PLZ (brutto)],
                                [HH der PLZ (netto)],
                                Datenstand,
                                Datenstand_Auflagen,
                                Datenquelle,
                                Status,
                                Gesplittet,
                                Stammtour
                            )
                            SELECT {tour.OccupancyUnitId},
                                    Medium_ID,
                                    [Medium-ID (WM)],
                                    [Medienname (Wasmuth)],
                                    Erscheintage,
                                    WoAnf,
                                    WoMi,
                                    WoEnd,
                                    Tarif_id,
                                    Tarif_Nr,
                                    Tarif_Name,
                                    gültig_ab,
                                    gültig_bis,
                                    [Ausgabenname (Wasmuth)],
                                    [Ausgaben-Nr],
                                    Tour_ID,
                                    Tour_Nr,
                                    '{tour.TourName}',
                                    PLZ,
                                    Ort,
                                    Ortsteil,
                                    {printNumber},
                                    [Auflage Info],
                                    [HH der PLZ (brutto)],
                                    [HH der PLZ (netto)],
                                    Datenstand,
                                    Datenstand_Auflagen,
                                    Datenquelle,
                                    Status,
                                    0,
                                    '{tourName}'
                            FROM dbo.Digitalisierte_BBE_Gebietslisten
                            WHERE ID = {area.Id}
                        ";
            var sqlCommand = new SqlCommand(sql, transaction.Connection, transaction);
            sqlCommand.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// It returns a list of tours that are inserted into a mother tour
    /// </summary>
    /// <param name="mediaId">The id of the media (newspaper)</param>
    /// <param name="motherTourName">"Stammtour"</param>
    /// <returns>
    /// A list of Tour objects.
    /// </returns>
    private List<Tour> GetInsertedSubTours(string mediaId, string motherTourName, SqlTransaction transaction)
    {
        string query = $@"
                              SELECT 
                                B.[BBE_ID] AS {nameof(Tour.OccupancyUnitId)},
                                M.[Gebietslisten_ID] AS {nameof(Tour.Id)},
                                B.[Medium_ID] AS {nameof(Tour.MediaId)}, 
                                B.[Ausgabe] AS {nameof(Tour.Issue)},
                                B.Tour as {nameof(Tour.TourName)}, 
                                B.Tour_ID as {nameof(Tour.TourId)}, 
                                B.Tour_Nr AS {nameof(Tour.TourNumber)},
                                B.Auflage AS {nameof(Tour.PrintNumber)},
                                B.Digitalisierung_von AS {nameof(Tour.Author)}
                              FROM dbo.Digitalisierte_BBE B
                              LEFT JOIN dbo.Digitalisierte_BBE_Split_Tour_Gebiets_Mapping M
                                    ON B.BBE_ID = M.BBE_ID
	                          WHERE Medium_ID = {mediaId}
	                          AND Stammtour = '{motherTourName}'
                            ";

        var sqlCommand = new SqlCommand(query, transaction.Connection, transaction);
        var reader = sqlCommand.ExecuteReader();

        var result = new List<Tour>();

        while (reader.Read())
        {
            Tour tour = new Tour();

            int id = 0;
            int.TryParse(reader[nameof(Tour.Id)].ToString(), out id);
            tour.OccupancyUnitId = int.Parse(reader[nameof(Tour.OccupancyUnitId)].ToString()); ;
            tour.Id = id;
            tour.TourName = reader[nameof(Tour.TourName)].ToString();
            tour.PrintNumber = int.Parse(reader[nameof(Tour.PrintNumber)].ToString());
            result.Add(tour);
        }
        reader.Close();
        if (result?.Count > 0)
        {
            return result;
        }
        return new List<Tour>();
    }

    /// <summary>
    /// It updates the database with the new split state of the mother tour
    /// </summary>
    /// <param name="motherTourName">The name of the mother tour</param>
    /// <param name="splittState">0 = not splitted, 1 = splitted</param>
    private void UpdateAreaSplitState(string motherTourName, int splittState)
    {
        string sql = $@"
                            UPDATE dbo.Digitalisierte_BBE_Gebietslisten
                            SET Gesplittet = {splittState}
                            WHERE Stammtour = '{motherTourName}'
                            ";

        DbConnection.Execute(sql);
    }
    private void UpdateAreaSplitState(string motherTourName, int splittState, SqlTransaction transaction)
    {
        string sql = $@"
                            UPDATE dbo.Digitalisierte_BBE_Gebietslisten
                            SET Gesplittet = {splittState}
                            WHERE Stammtour = '{motherTourName}'
                            ";

        var sqlCommand = new SqlCommand(sql, transaction.Connection, transaction);
        sqlCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// It deletes all records from the table Digitalisierte_BBE_Details where BBE_ID =
    /// occupancyUnitId and then inserts all records from the occupancyUnits list into the table
    /// Digitalisierte_BBE_Details.
    /// </summary>
    /// <param name="occupancyUnits">List<OccupancyUnit></param>
    /// <param name="occupancyUnitId">is the primary key of the table Digitalisierte_BBE</param>
    public void InsertOccupancyUnitsDetail(List<OccupancyUnit> occupancyUnits, string occupancyUnitId)
    {

        DeleteOccupancyUnitsDetail(occupancyUnitId);
        foreach (var unit in occupancyUnits)
        {
            string sql = $@"
                        INSERT INTO [dbo].[Digitalisierte_BBE_Details](
                            [BBE_ID]
                        ,[mPLZ]
                        ,[Stand]
                        ,[digitalisiert_am]
                        ,[digitalisiert_von]
                        ) VALUES (
                            {occupancyUnitId}
                        ,'{unit.MicroZipCode}'
                        ,'{unit.BorderStatus}'
                        ,'{unit.LastModified}'
                        ,'{unit.Author}'
                        )                          
";
            try
            {
                DbConnection.Execute(sql);
            }
            catch (SqlException)
            {
                DeleteOccupancyUnitsRecord(occupancyUnitId, unit.MicroZipCode);
                DbConnection.Execute(sql);
            }
        }
    }

    /// <summary>
    /// Delete from the table Digitalisierte_BBE_Details where the BBE_ID is equal to the
    /// occupancyUnitId
    /// </summary>
    /// <param name="occupancyUnitId">"1"</param>
    public void DeleteOccupancyUnitsDetail(string occupancyUnitId)
    {
        string sql = $@"
                            Delete
                            From [dbo].[Digitalisierte_BBE_Details]
                            WHERE BBE_ID = {occupancyUnitId}
                            ";
        DbConnection.Execute(sql);
    }

    /// <summary>
    /// It deletes a record from a table in a database
    /// </summary>
    /// <param name="occupancyUnitId">This is the ID of the occupancy unit that is being
    /// deleted.</param>
    /// <param name="microZipCode">string</param>
    private void DeleteOccupancyUnitsRecord(string occupancyUnitId, string microZipCode)
    {
        string sql = $@"
                            Delete
                            From [dbo].[Digitalisierte_BBE_Details]
                            WHERE BBE_ID = {occupancyUnitId}
                                AND mPLZ = {microZipCode}
                            ";
        DbConnection.Execute(sql);
    }

    /// <summary>
    /// It updates the table Digitalisierte_BBE with the sum of the column HH_BRUTTO from the table
    /// Geodaten_HAUSHALTE_MPLZ.
    /// </summary>
    /// <param name="occupancyUnitId">The id of the occupancy unit</param>
    /// <param name="userName">The user who is currently logged in</param>
    public void SaveDigitizedOccupancyUnits(string occupancyUnitId, string userName)
    {
        if (occupancyUnitId != null)
        {
            string sql = $@"
                                UPDATE b
                                SET 
                                b.HH_Brutto = t.HH_Brutto,
                                b.Digitalisierung_Status_ID = 9,
                                b.Digitalisierung_von = '{userName}',
                                b.Digitalisierung_letzte_Bearbeitung = GetDate()
                                FROM dbo.Digitalisierte_BBE b
                                    INNER JOIN
                                    (
                                        SELECT b.BBE_ID,
                                               SUM(hh.HH_BRUTTO) AS HH_Brutto
                                        FROM dbo.Digitalisierte_BBE b
                                            LEFT JOIN dbo.Digitalisierte_BBE_Details d
                                                ON d.BBE_ID = b.BBE_ID
                                            LEFT JOIN dbo.Geodaten_HAUSHALTE_MPLZ hh
                                                ON hh.mPLZ = d.mPLZ
                                        GROUP BY b.BBE_ID
                                    ) t
                                        ON t.BBE_ID = b.BBE_ID
                                WHERE b.BBE_ID = {occupancyUnitId}
                                ";

            DbConnection.Execute(sql);
        }
    }

    /// <summary>
    /// It executes a stored procedure called "Create_Advertisement_Geography" with the parameters
    /// "Werbegebiets_Nr" and "UseMediaCenter_TEST_DB" and the values "advertisementAreNumber" and
    /// "useTestDatabase" respectively
    /// </summary>
    /// <param name="advertisementAreNumber">The number of the advertisement area.</param>
    /// <param name="useTestDatabase">0 = false, 1 = true</param>
    public void ExecuteCreateAdvertisementGeographyStoredProcedure(int advertisementAreNumber, byte useTestDatabase = 0)
    {
        var procedure = "Create_Advertisement_Geography";
        var values = new { Werbegebiets_Nr = advertisementAreNumber, UseMediaCenter_TEST_DB = useTestDatabase };
        DbConnection.Execute(procedure, values, null, 1000, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// It checks if there are any postal codes in the list of occupancy units that are shared by
    /// more than one occupancy unit
    /// </summary>
    /// <param name="units">List of integers</param>
    /// <returns>
    /// The result is a list of all the units that are intersecting.
    /// </returns>
    public bool CheckForIntersectingOccupancyUnits(List<int> units)
    {
        string sql = $@"
                            SELECT distinct 1 FROM 
                            (
                            SELECT 
	                            COUNT(mPLZ) treffer
                            FROM dbo.Digitalisierte_BBE_Details
                            WHERE BBE_ID IN ({string.Join(",", units)})
                            GROUP BY mPLZ
                            ) t
                            WHERE treffer > 1
                            ";

        var result = DbConnection.ExecuteScalar<int>(sql);
        if (result == 1)
            return true;
        return false;
    }

    /// <summary>
    /// It updates a database table with the current user's name and the current date and time
    /// </summary>
    /// <param name="occupancyUnitId">The ID of the occupancy unit</param>
    public void SaveEvaluation(string occupancyUnitId)
    {
        string sql = $@"
                            UPDATE dbo.Digitalisierte_BBE
                                SET Geprüft_von = '{Environment.UserName}',
                                    Geprüft_am = '{DateTime.Now}'
                            WHERE BBE_ID = {occupancyUnitId}
                        ";

        DbConnection.Execute(sql);
    }

    /// <summary>
    /// It executes a stored procedure on a SQL Server database
    /// </summary>
    /// <param name="customerId">The customer id</param>
    /// <param name="branchNumber">The branch number of the branch to be planned</param>
    /// <param name="mindNeigbours">If true, the planner will take into account the neighbours of
    /// the branch.</param>
    public void ExecuteAutoPlanner_SingleBranch(int customerId, string branchNumber, bool mindNeigbours)
    {

        var procedure = "AutoAreaPlanner_SingleBranch";
        var values = new
        {
            Kunden_ID = customerId,
            Filial_Nr = branchNumber,
            Planung_von = Environment.UserName,
            NachbarfilialenBeachten = mindNeigbours ? 1 : 0
        };
        DbConnection.Execute(procedure, values, null, 1000, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// It executes a stored procedure on a SQL Server database
    /// </summary>
    /// <param name="customerId">The customer's ID</param>
    /// <param name="branchNumber">The branch number of the branch you want to plan</param>
    /// <param name="mindNeigbours">If true, the planner will try to avoid placing the same branch
    /// next to each other.</param>
    /// <param name="targetNumberOfCopies">The number of copies that should be distributed to the
    /// branch</param>
    public void ExecuteAutoPlanner_SingleBranch(int customerId, string branchNumber, bool mindNeigbours, int targetNumberOfCopies)
    {

        var procedure = "AutoAreaPlanner_SingleBranch";
        var values = new
        {
            Kunden_ID = customerId,
            Filial_Nr = branchNumber,
            Zielauflage = targetNumberOfCopies,
            Planung_von = Environment.UserName,
            NachbarfilialenBeachten = mindNeigbours ? 1 : 0
        };
        DbConnection.Execute(procedure, values, null, 1000, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// It returns a list of strings from a SQL query
    /// </summary>
    /// <returns>
    /// A list of strings.
    /// </returns>
    public List<string> GetAutoPlannerData()
    {
        string sql = $@"
                            SELECT d.Geokey
                            FROM dbo.Werbegebietsplanung_Details d
                                INNER JOIN dbo.Werbegebietsplanung w
                                    ON w.Planungs_ID = d.Planungs_ID
                            WHERE d.Planungs_ID =
                            (
                                SELECT MAX(Planungs_ID)FROM dbo.Werbegebietsplanung
                            )
                                  AND w.Planung_von like '%{Environment.UserName}%';
                            ";

        var result = DbConnection.Query<string>(sql).ToList();
        if (result?.Count > 0)
            return result;
        return new List<string>();
    }

    public async Task<List<string>> GetAutoPlannerDataAsync()
    {
        string sql = $@"
                            SELECT d.Geokey
                            FROM dbo.Werbegebietsplanung_Details d
                                INNER JOIN dbo.Werbegebietsplanung w
                                    ON w.Planungs_ID = d.Planungs_ID
                            WHERE d.Planungs_ID =
                            (
                                SELECT MAX(Planungs_ID)FROM dbo.Werbegebietsplanung
                            )
                                  AND w.Planung_von like '%{Environment.UserName}%';
                            ";

        var result = (await DbConnection.QueryAsync<string>(sql)).ToList();
        if (result?.Count > 0)
            return result;
        return new List<string>();
    }

    /// <summary>
    /// It returns a string that is either "vollständig", "teilweise" or "nicht" depending on the
    /// number of rows in a table that have a certain value in a certain column
    /// </summary>
    /// <param name="mediaId">int</param>
    /// <returns>
    /// A string.
    /// </returns>
    public string GetZipCodeSharpnesState(int mediaId)
    {
        string query = $@"
                            SELECT
                            IIF(Scharf > 0 AND Unscharf > 0, 'teilweise', IIF(Scharf = 0 AND Unscharf > 0, 'nicht', 'vollständig'))
                            FROM 
                            (
                                SELECT 
		                            SUM(CASE WHEN PLZ_Scharf = 'Scharf' THEN 1 ELSE 0 end) AS Scharf,
		                            SUM(CASE WHEN PLZ_Scharf = 'unscharf' THEN 1 ELSE 0 end) AS Unscharf
                                FROM
                                (
                                    SELECT IIF(VG.Auflage_Info = '1: PLZ genau', 'scharf', 'unscharf') PLZ_Scharf
                                    FROM dbo.Verbreitungsgebiete_mit_Geometrie VG
                                        LEFT JOIN dbo.Digitalisierte_BBE BBE
                                            ON BBE.BBE_ID = VG.BBE_ID
                                    WHERE VG.Medium_ID = {mediaId}
                                            AND ISNULL(BBE.Archiv, 0) = 0
                                            AND VG.Datenquelle = 'Wasmuth'
			                                AND VG.Belegungseinheit = 'PLZ'
                                ) as t
                            ) AS src
                            ";

        string result = DbConnection.ExecuteScalar<string>(query);
        if (result != null)
            return result;
        return "nicht";
    }

    public async Task<string> GetZipCodeSharpnesStateAsync(int mediaId)
    {
        string query = $@"
                            SELECT
                            IIF(Scharf > 0 AND Unscharf > 0, 'teilweise', IIF(Scharf = 0 AND Unscharf > 0, 'nicht', 'vollständig'))
                            FROM 
                            (
                                SELECT 
		                            SUM(CASE WHEN PLZ_Scharf = 'Scharf' THEN 1 ELSE 0 end) AS Scharf,
		                            SUM(CASE WHEN PLZ_Scharf = 'unscharf' THEN 1 ELSE 0 end) AS Unscharf
                                FROM
                                (
                                    SELECT IIF(VG.Auflage_Info = '1: PLZ genau', 'scharf', 'unscharf') PLZ_Scharf
                                    FROM dbo.Verbreitungsgebiete_mit_Geometrie VG
                                        LEFT JOIN dbo.Digitalisierte_BBE BBE
                                            ON BBE.BBE_ID = VG.BBE_ID
                                    WHERE VG.Medium_ID = {mediaId}
                                            AND ISNULL(BBE.Archiv, 0) = 0
                                            AND VG.Datenquelle = 'Wasmuth'
			                                AND VG.Belegungseinheit = 'PLZ'
                                ) as t
                            ) AS src
                            ";

        string result = await DbConnection.ExecuteScalarAsync<string>(query);
        if (result != null)
            return result;
        return "nicht";
    }

    //public SqlTransaction BeginTransaction()
    //{
    //    if (DbConnection.State != ConnectionState.Open)
    //        DbConnection.Open();

    //    return (DbConnection.BeginTransaction() as SqlTransaction);
    //}
    public SqlTransaction BeginTransaction()
    {
        if (DbConnection.State != ConnectionState.Open)
            DbConnection.Open();

        return (DbConnection.BeginTransaction() as SqlTransaction);
    }

    public void CommitTransaction(SqlTransaction transaction)
    {
        transaction.Commit();
        DbConnection.Close();
    }

    public void RollbackTransaction(SqlTransaction transaction)
    {
        transaction.Rollback();
        DbConnection.Close();
    }

    public void InsertTourAreaMapping(string tourName, List<Tour> subTours, SqlTransaction transaction)
    {
        var mediaId = subTours.FirstOrDefault().MediaId;
        var insertedSubTours = GetInsertedSubTours(mediaId, tourName, transaction);
        foreach (var tour in subTours)
        {
            var occupancyUnitId = insertedSubTours.Where(t => t.TourName == tour.TourName).FirstOrDefault().OccupancyUnitId;
            string sql = $@"
                            INSERT INTO dbo.Digitalisierte_BBE_Split_Tour_Gebiets_Mapping
                            (
                                BBE_ID,
                                Gebietslisten_ID
                            )
                            VALUES 
                            (
                                ${occupancyUnitId},
                                ${tour.Id}
                            );
                            ";
            var sqlCommand = new SqlCommand(sql, transaction.Connection, transaction);
            sqlCommand.ExecuteNonQuery();
        }
    }
}
