using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Windows.Forms;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the old repository for interaction with geometry data.
/// </summary>
public class GeometryRepository : Repository<Geometry>, IGeometryRepository
{
    public GeometryRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.Geoservices)
    {
    }

    /// <summary>
    /// It takes a list of zip codes and returns a single polygon
    /// </summary>
    /// <param name="zipCodes">List of zip codes</param>
    /// <param name="mediaName">The name of the media</param>
    /// <returns>
    /// A DbGeography object.
    /// </returns>
    public DbGeography GetSinglePolygon(List<string> zipCodes, string mediaName)
    {
        string zipCodesString = "'" + string.Join("', '", zipCodes) + "'";
        string query = $@"
                            select dbo.Geographyunionaggregate(Geom, 0).Reduce(5).ToString()
                            from Geometries 
                            where Stand = '31.12.2016' and Geokey in({zipCodesString})
                            ";
        try
        {
            var wktArea = DbConnection.ExecuteScalar<string>(query, null, null, 300);
            if (wktArea.ToLower().Contains("multipolygon"))
            {
                return DbGeography.MultiPolygonFromText(wktArea, 4326);
            }
            else
            {
                if (wktArea.ToLower().Contains("polygon"))
                {
                    try
                    {
                        return DbGeography.PolygonFromText(wktArea, 4326);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        catch (SqlException ex)
        {
            MessageBox.Show($@"Das Gebiet {mediaName} konnte nicht geladen werden. {ex.StackTrace}", "Ladefehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        return null;
    }
}
