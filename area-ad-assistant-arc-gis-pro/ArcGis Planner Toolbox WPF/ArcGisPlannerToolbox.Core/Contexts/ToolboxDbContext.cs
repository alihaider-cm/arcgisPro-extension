using ArcGisPlannerToolbox.Core.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;

namespace ArcGisPlannerToolbox.Core.Contexts;

public class ToolboxDbContext : IDbContext
{
    public IDbConnection CreateDbConnection(string connectionString, DbConnectionName dbName)
    {
        string environment = System.Environment.GetEnvironmentVariable("Environment") ?? "Production";
        try
        {
            if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLServer);
                return new SqlConnection(connectionString);
            }
            else if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                if (dbName == DbConnectionName.GeoInsights)
                {
                    SimpleCRUD.SetDialect(SimpleCRUD.Dialect.PostgreSQL);
                    return new NpgsqlConnection(connectionString);
                }
                else
                {
                    SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLServer);
                    return new SqlConnection(connectionString);
                }
            }
            else
            {
                throw new ArgumentNullException("Environment is undefined");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.StackTrace, ex.Message);
            throw new ArgumentException("Database not available or not accessible");
        }
    }
}
