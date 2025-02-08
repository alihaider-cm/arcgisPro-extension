using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace ArcGisPlannerToolbox.Persistence.Repositories
{
    /// <summary>
    /// This is the repository for interaction with analyisis data.
    /// </summary>
    public class AnalysisRepository : Repository<Analysis>, IAnalysisRepository
    {
        public AnalysisRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.ANALYSEDATEN)
        {
        }

        /// <summary>
        /// It returns a list of Analysis objects, which are created from the data in the database
        /// </summary>
        /// <param name="customerId">1</param>
        /// <returns>
        /// A list of Analysis objects.
        /// </returns>
        public List<Analysis> GetAnalysisByCustomerId(int customerId)
        {
            string sql = $@"
                            SELECT Analyse_ID as {nameof(Analysis.Analyse_ID)}
                                    , Analyse_Name as {nameof(Analysis.Analyse_Name)}
                                    , Kunde_ID as {nameof(Analysis.Kunden_ID)}
                            FROM dbo._System_Analysen
                            WHERE Kunde_ID = {customerId}
                            ";

            var result = DbConnection.Query<Analysis>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<Analysis>();
        }
    }
}
