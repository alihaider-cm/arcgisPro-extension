using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArcGisPlannerToolbox.WPF.Constants;
using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the base repository class.
/// </summary>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly IDbConnection DbConnection;
    protected readonly string _environment;

    /* Creating a connection to the database. */
    public Repository(IDbContext dbConnection, DbConnectionName dbName)
    {
        var connection = AppConfig.GetConfiguration(dbName.ToString());
        DbConnection = dbConnection.CreateDbConnection(connection, dbName);
        _environment = System.Environment.GetEnvironmentVariable("Environment") ?? "Production"; 
    }

    /// <summary>
    /// It takes an entity, and inserts it into the database
    /// </summary>
    /// <param name="TEntity">The entity type that the repository is for.</param>
    public void Add(TEntity entity)
    {
        DbConnection.Insert(entity);
    }

    /// <summary>
    /// > AddRange() adds a range of entities to the DbSet
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    public void AddRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Add(entity);
        }
    }

    /// <summary>
    /// It returns a list of entities that match the whereExpression and parameters
    /// </summary>
    /// <param name="whereExpression">"Id = @Id"</param>
    /// <param name="parameters"></param>
    /// <returns>
    /// A list of TEntity objects.
    /// </returns>
    public IEnumerable<TEntity> Find(string whereExpression, object parameters)
    {
        return DbConnection.GetList<TEntity>(whereExpression, parameters);
    }

    /// <summary>
    /// It returns a list of entities that match the condition
    /// </summary>
    /// <param name="condition">The condition to be used in the WHERE clause.</param>
    /// <returns>
    /// A list of TEntity objects.
    /// </returns>
    public IEnumerable<TEntity> Find(string condition)
    {
        return DbConnection.GetList<TEntity>(condition);
    }

    /// <summary>
    /// It takes a string as a parameter and returns a list of entities
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <returns>
    /// A list of TEntity objects.
    /// </returns>
    public IEnumerable<TEntity> GetListByQuery(string query)
    {
        return DbConnection.Query<TEntity>(query);
    }

    /// <summary>
    /// Execute a SQL statement against the database
    /// </summary>
    /// <param name="sql">The SQL statement to execute.</param>
    /// <param name="parameters"></param>
    /// <returns>
    /// The number of rows affected by the query.
    /// </returns>
    public int Execute(string sql, object parameters = null)
    {
        return DbConnection.Execute(sql, parameters);
    }

    /// <summary>
    /// It returns a single entity from the database based on the id
    /// </summary>
    /// <param name="id">The id of the entity to retrieve.</param>
    /// <returns>
    /// The entity with the specified id.
    /// </returns>
    public TEntity Get(int id)
    {
        return DbConnection.Get<TEntity>(id);
    }

    /// <summary>
    /// It returns a list of all the entities in the database
    /// </summary>
    /// <returns>
    /// A list of TEntity objects.
    /// </returns>
    public IEnumerable<TEntity> GetAll()
    {
        return DbConnection.GetList<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await DbConnection.GetListAsync<TEntity>();
    }

    /// <summary>
    /// It removes an entity from the database
    /// </summary>
    /// <param name="TEntity">The entity type that the repository is for.</param>
    public void Remove(TEntity entity)
    {
        DbConnection.Delete(entity);
    }

    /// <summary>
    /// It removes a range of entities from the database
    /// </summary>
    /// <param name="entities">The entities to remove.</param>
    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Remove(entity);
        }
    }
}
