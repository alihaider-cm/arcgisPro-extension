using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the repository for interaction with map view data.
/// </summary>
public class MapViewMediaRepository : Repository<Media>, IMapViewMediaRepository
{
    public MapViewMediaRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.GeoInsights)
    {
    }

    /// <summary>
    /// It takes a list of coordinates and returns a list of media ids
    /// </summary>
    /// <param name="coords">[ "8.5478515625,47.835290982707", "8.5478515625,47.835290982707",
    /// "8.5478515625,47.835290982707", "8</param>
    /// <returns>
    /// A list of integers.
    /// </returns>
    public async Task<List<int>> FindMediaIdByMapView(List<string> coords)
    {
        if (coords is not null && coords?.Count > 0)
        {
            string query = "";
            if (_environment == "Development")
            {
                query = String.Format(@"
                                Select distinct m.medium_id
                                From
                                [dbo].[werbegebiete_mit_geometrien] vg
                                inner Join dbo.vw_Medien m 
									On m.medium_ID=vg.medium_ID
                                where vg.geom.MakeValid().STIntersects(geometry::STGeomFromText('POLYGON(({0}
                                ,{1}
                                ,{2}
                                ,{3}
                                ,{4}))', 4326).MakeValid()) = 1"
                                , coords[0], coords[1], coords[2], coords[3], coords[0]);
            }
            else if (_environment == "Production")
            {
                query = String.Format(@"Select distinct m.medium_id
                                From
                                public.verbreitungsgebiete_mit_geometrie vg
                                inner Join public.medien m On m.medium_ID=vg.medium_ID
                                Where ST_Intersects(vg.geom,
                                ST_GeomFromText('POLYGON(( {0}
                                ,{1}
                                ,{2}
                                ,{3}
                                ,{4}
                                ))',4326)
                                )", coords[0], coords[1], coords[2], coords[3], coords[0]);
            }

            var mediaIds = await DbConnection.QueryAsync<int>(query);

            return mediaIds.ToList();

        }
        return null;
    }

    /// <summary>
    /// It takes a list of coordinates and an id and returns a list of strings
    /// </summary>
    /// <param name="coords">a list of coordinates (lat/long)</param>
    /// <param name="id">the id of the medium</param>
    /// <returns>
    /// A list of strings.
    /// </returns>
    public List<string> FindIssuesByMapView(List<string> coords, int id)
    {
        if (coords is null) return null;

        string query = "";
        if (_environment == "Development")
        {
            query = string.Format(@"Select distinct vg.Ausgabe
                                From
                                dbo.verbreitungsgebiete_mit_geometrie vg
                                inner Join dbo.medien m On m.medium_ID=vg.medium_ID
                                Where vg.geom.MakeValid().STIntersects(geometry::STGeomFromText(
                                'POLYGON(( {0}
                                ,{1}
                                ,{2}
                                ,{3}
                                ,{4}
                                ))',4326))
                                And m.medium_ID = {5}", coords[0], coords[1], coords[2], coords[3], coords[0], id);
        }
        else if (_environment == "Production")
        {
            query = String.Format(@"Select distinct vg.Ausgabe
                                From
                                public.verbreitungsgebiete_mit_geometrie vg
                                inner Join public.medien m On m.medium_ID=vg.medium_ID
                                Where ST_Intersects(vg.geom,
                                ST_GeomFromText('POLYGON(( {0}
                                ,{1}
                                ,{2}
                                ,{3}
                                ,{4}
                                ))',4326)
                                )
                                And m.medium_ID = {5}", coords[0], coords[1], coords[2], coords[3], coords[0], id);
        }
         

        var issues = DbConnection.Query<string>(query);

        return issues.ToList();
    }
}
