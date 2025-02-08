using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the repository for interaction with data from the Gebietsassistent database.
/// </summary>
public class GebietsassistentRepository : Repository<DistributionMedia>, IGebietsassistentRepository
{
    public GebietsassistentRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.GEBIETSASSISTENT)
    {
    }

    /// <summary>
    /// It returns a list of DistributionMedia objects, which are filled with data from a SQL Server
    /// database
    /// </summary>
    /// <returns>
    /// A list of DistributionMedia objects.
    /// </returns>
    public List<DistributionMedia> GetDestributionMedia()
    {
        string sql = $@"
                            SELECT Mediengattung as {nameof(DistributionMedia.DistMedia)},
                                   Medientyp as {nameof(DistributionMedia.MediaType)},
                                   Verteiltyp as {nameof(DistributionMedia.SpreadingType)} 
                            FROM dbo.Mediengattungen
                            ";

        var result = DbConnection.Query<DistributionMedia>(sql).ToList();
        if (result.Count > 0)
        {
            return result;
        }
        return new List<DistributionMedia>();
    }
    public async Task<List<DistributionMedia>> GetDestributionMediaAsync()
    {
        string sql = $@"
                            SELECT Mediengattung as {nameof(DistributionMedia.DistMedia)},
                                   Medientyp as {nameof(DistributionMedia.MediaType)},
                                   Verteiltyp as {nameof(DistributionMedia.SpreadingType)} 
                            FROM dbo.Mediengattungen
                            ";

        var result = (await DbConnection.QueryAsync<DistributionMedia>(sql)).ToList();
        if (result.Count > 0)
        {
            return result;
        }
        return new List<DistributionMedia>();
    }
}
