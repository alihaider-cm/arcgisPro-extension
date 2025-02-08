using ArcGisPlannerToolbox.Core.Models;
using System.Data;

namespace ArcGisPlannerToolbox.Core.Contexts;

public interface IDbContext
{
    IDbConnection CreateDbConnection(string connectionString, DbConnectionName dbName);
}
