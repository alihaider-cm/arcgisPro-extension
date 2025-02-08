using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the repository for interaction with advertisement area statistics.
/// </summary>
public class AdvertisementAreaStatisticsRepository : Repository<AdvertisementAreaStatistics>, IAdvertisementAreaStatisticsRepository
{
    public AdvertisementAreaStatisticsRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.MediaCenter)
    {
    }

    private string _database;

    /// <summary>
    /// This function sets the database name
    /// </summary>
    /// <param name="database">The name of the database you want to connect to.</param>
    public void SetDataBase(string database)
    {
        _database = database;
    }

    /// <summary>
    /// It returns a list of AdvertisementAreaStatistics objects, which are created from the results
    /// of a query to a database.
    /// </summary>
    /// <param name="CustomerBranch"></param>
    /// <returns>
    /// A list of AdvertisementAreaStatistics objects.
    /// </returns>
    public List<AdvertisementAreaStatistics> GetCustomerStatisticsByBranch(CustomerBranch branch)
    {
        string query = $@"
                            SELECT WSWJF.[Filial_ID] AS {nameof(AdvertisementAreaStatistics.Filial_ID)}
                                   ,WSWJF.[Kunden_ID] AS {nameof(AdvertisementAreaStatistics.Kunden_ID)}
                                   ,WSWJF.[Kundenname] AS {nameof(AdvertisementAreaStatistics.Kundenname)}
                                   ,WSWJF.[Vertriebslinie] AS {nameof(AdvertisementAreaStatistics.Vertriebslinie)}
                                   ,WSWJF.[Filialname] AS {nameof(AdvertisementAreaStatistics.Filialname)}
                                   ,WSWJF.[Werbegebiets_Nr] AS {nameof(AdvertisementAreaStatistics.Werbegebiets_Nr)}
                                   ,WSWJF.[Gebietsbezeichnung] AS {nameof(AdvertisementAreaStatistics.Gebietsbezeichnung)}
                                   ,WS.[Statusname] AS {nameof(AdvertisementAreaStatistics.Werbegebietsstatus)}
                                   ,WSWJF.[gültig_ab_Jahr_KW] AS {nameof(AdvertisementAreaStatistics.gültig_ab_Jahr_KW)}
                                   ,WSWJF.[gültig_bis_Jahr_KW] AS {nameof(AdvertisementAreaStatistics.gültig_bis_Jahr_KW)}
                                   ,WSWJF.[Basisgeometrie] AS {nameof(AdvertisementAreaStatistics.Basisgeometrie)}
                                   ,WSWJF.[Aktionstyp] AS {nameof(AdvertisementAreaStatistics.Aktionstyp)}
                                   ,WSWJF.[Gesamtauflage] AS {nameof(AdvertisementAreaStatistics.Gesamtauflage)}
                                   ,WSWJF.[Anzahl_Medien] AS {nameof(AdvertisementAreaStatistics.Anzahl_Medien)}
                                   ,WSWJF.[ISIS_sichtbar] AS {nameof(AdvertisementAreaStatistics.ISIS_sichtbar)}
                            FROM {_database}.[dbo].[Werbegebietsverwaltung_Statistik_Werbegebiete_je_Filiale] WSWJF
                                LEFT JOIN {_database}.dbo.Werbegebietsstatus WS
                                    ON WsWJF.Werbegebietsstatus_ID = Ws.Werbegebietsstatus_ID 
                            WHERE WSWJF.Kunden_ID = '{branch.Kunden_ID}'
                            AND WSWJF.Filial_ID = '{branch.Filial_ID}';
                            ";

        var results = DbConnection
            .Query<AdvertisementAreaStatistics>(query)
            .OrderByDescending(a => a.Werbegebietsstatus)
            .ToList();
        if (results != null)
        {
            return results;
        }
        return new List<AdvertisementAreaStatistics>();
    }

    /// <summary>
    /// It returns a list of ActionTypes from the database
    /// </summary>
    /// <returns>
    /// A list of ActionType objects.
    /// </returns>
    public List<ActionType> GetActionTypes()
    {
        string query = $@"
                            SELECT [Aktionstyp] AS {nameof(ActionType.ActionTypeName)}
                                ,[aktiv] AS {nameof(ActionType.Active)}
                                ,[Letzte_Änderung_von] AS {nameof(ActionType.ChangedBy)}
                                ,[Letzte_Änderung_am] AS {nameof(ActionType.LastChanged)}
                            FROM dbo.Werbegebietsverwaltung_Aktionstypen
                            GROUP BY [Aktionstyp]
                                ,[aktiv]
                                ,[Letzte_Änderung_von]
                                ,[Letzte_Änderung_am]
                            ";

        List<ActionType> results;
        try
        {
            results = DbConnection
            .Query<ActionType>(query).ToList();

        }
        catch (Exception)
        {
            results = null;
        }
        if (results != null)
        {
            return results;
        }
        return new List<ActionType>();
    }

    /// <summary>
    /// It returns a list of ActionTypes from the database
    /// </summary>
    /// <param name="customerId">int</param>
    /// <returns>
    /// A list of ActionType objects.
    /// </returns>
    public List<ActionType> GetActionTypes(int customerId)
    {
        string query = $@"
                              SELECT [Aktionstyp] AS {nameof(ActionType.ActionTypeName)}
                              ,[aktiv] AS {nameof(ActionType.Active)}
                              ,[Letzte_Änderung_von] AS {nameof(ActionType.ChangedBy)}
                              ,[Letzte_Änderung_am] AS {nameof(ActionType.LastChanged)}
                              FROM dbo.Werbegebietsverwaltung_Aktionstypen
                              WHERE Kunden_Id = {customerId}
                            ";

        List<ActionType> results;
        try
        {
            results = DbConnection
            .Query<ActionType>(query).ToList();

        }
        catch (Exception)
        {
            results = null;
        }
        if (results != null)
        {
            return results;
        }
        return new List<ActionType>();
    }
}
