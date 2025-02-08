using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the repository for interaction with customer data.
/// </summary>
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.MediaCenter)
    {
    }

    /// <summary>
    /// It returns a list of customers from the database
    /// </summary>
    /// <returns>
    /// A list of customers.
    /// </returns>
    public List<Customer> GetCustomers()
    {
        string query = $@"
                            SELECT [Kunden_ID] AS {nameof(Customer.Kunden_ID)}
                                  ,[Kunde] AS {nameof(Customer.Kunde)}
                                  ,[Firmenname] AS {nameof(Customer.Firmenname)}
                                  ,[Straße] AS {nameof(Customer.Straße)}
                                  ,[PLZ] AS {nameof(Customer.PLZ)}
                                  ,[ORT] AS {nameof(Customer.ORT)}
                                  ,[Bundesland] AS {nameof(Customer.Bundesland)}
                                  ,[Kunden_Nr] AS {nameof(Customer.Kunden_Nr)}
                                  ,[Organisations_Nr] AS {nameof(Customer.Organisations_Nr)}
                                  ,[Adress_Id] AS {nameof(Customer.Adress_Id)}
                                  ,[ISIS_aktiv] AS {nameof(Customer.ISIS_aktiv)}
                                  ,[X_GK3] AS {nameof(Customer.X_GK3)}
                                  ,[Y_GK3] AS {nameof(Customer.Y_GK3)}
                            FROM [dbo].[v_SuperOffice_Kunden] s
                            INNER JOIN dbo.v_Analysedaten_Kunden a
	                            ON s.Kunden_ID = a.Kunde_ID
                            ";

        var results = DbConnection.Query<Customer>(query).ToList();
        if (results != null)
        {
            return results;
        }
        return new List<Customer>();
    }

    /// <summary>
    /// It returns a list of CustomerBranch objects, which are the branches of a customer
    /// </summary>
    /// <param name="customerId">int</param>
    /// <returns>
    /// A list of CustomerBranch objects.
    /// </returns>
    public List<CustomerBranch> GetBranchByCustomer(int customerId)
    {
        string query = $@"
                            SELECT [Kunden_ID] AS {nameof(CustomerBranch.Kunden_ID)}
                                ,[Kundenname] AS {nameof(CustomerBranch.Kundenname)}
                                ,[Vertriebslinie] AS {nameof(CustomerBranch.Vertriebslinie)}
                                ,[Filial_ID] AS {nameof(CustomerBranch.Filial_ID)}
                                ,[Filial_Nr] AS {nameof(CustomerBranch.Filial_Nr)}
                                ,[Filialname] AS {nameof(CustomerBranch.Filialname)}
                                ,[Straße] AS {nameof(CustomerBranch.Straße)}
                                ,[PLZ] AS {nameof(CustomerBranch.PLZ)}
                                ,[ORT] AS {nameof(CustomerBranch.ORT)}
                                ,[X_WGS84] AS {nameof(CustomerBranch.X_WGS84)}
                                ,[Y_WGS84] AS {nameof(CustomerBranch.Y_WGS84)}
                            FROM [MEDIACENTER].[dbo].[Filialen_und_Konzerne]
                            WHERE Kunden_ID = {customerId}
                            ";

        var results = DbConnection.Query<CustomerBranch>(query).ToList();
        if (results != null)
        {
            return results;
        }
        return new List<CustomerBranch>();
    }

    /// <summary>
    /// It returns a list of CustomerBranch objects from the database, based on a customerId and a
    /// list of branch numbers
    /// </summary>
    /// <param name="customerId">int</param>
    /// <param name="branchNumbers">List<string></param>
    /// <returns>
    /// A list of CustomerBranch objects.
    /// </returns>
    public List<CustomerBranch> GetBranchesByBranchNumbers(int customerId, List<string> branchNumbers)
    {
        string andClause = CreateAndClauseFromList(branchNumbers);
        string sql = $@"
                            SELECT [Kunden_ID] AS {nameof(CustomerBranch.Kunden_ID)}
                                ,[Kundenname] AS {nameof(CustomerBranch.Kundenname)}
                                ,[Vertriebslinie] AS {nameof(CustomerBranch.Vertriebslinie)}
                                ,[Filial_ID] AS {nameof(CustomerBranch.Filial_ID)}
                                ,[Filial_Nr] AS {nameof(CustomerBranch.Filial_Nr)}
                                ,[Filialname] AS {nameof(CustomerBranch.Filialname)}
                                ,[Straße] AS {nameof(CustomerBranch.Straße)}
                                ,[PLZ] AS {nameof(CustomerBranch.PLZ)}
                                ,[ORT] AS {nameof(CustomerBranch.ORT)}
                                ,[X_WGS84] AS {nameof(CustomerBranch.X_WGS84)}
                                ,[Y_WGS84] AS {nameof(CustomerBranch.Y_WGS84)}
                            FROM [MEDIACENTER].[dbo].[Filialen_und_Konzerne]
                            Where Kunden_ID = {customerId}
                            {andClause}
                            ";

        var result = DbConnection.Query<CustomerBranch>(sql).ToList();
        if (result.Count > 0)
        {
            return result;
        }
        return new List<CustomerBranch>();
    }

    /// <summary>
    /// It takes a list of strings and returns a string that can be used in a SQL query
    /// </summary>
    /// <param name="branchNumbers">List<string></param>
    /// <returns>
    /// A string
    /// </returns>
    private string CreateAndClauseFromList(List<string> branchNumbers)
    {
        if (branchNumbers.Count > 0)
        {
            string andClause = "AND Filial_Nr in (";
            foreach (string branch in branchNumbers)
            {
                andClause += $"'{branch}',";
            }
            andClause = andClause.Remove(andClause.Length - 1);
            return andClause += ")";
        }
        return "";
    }

    /// <summary>
    /// It returns the nearest 15 customer branches to the given customer branch
    /// </summary>
    /// <param name="customerId">int</param>
    /// <param name="CustomerBranch"></param>
    /// <returns>
    /// A list of CustomerBranch objects.
    /// </returns>
    public List<CustomerBranch> GetNearestCustomerBranches(int customerId, CustomerBranch customerBranch)
    {
        float la = customerBranch.Y_WGS84;
        float lo = customerBranch.X_WGS84;
        string y;
        string x;
        if (la != 0 && lo != 0 && la.ToString().Contains(",") && lo.ToString().Contains(","))
        {
            var substringsLa = la.ToString().Split(new string[] { "," }, StringSplitOptions.None);
            var substringsLo = lo.ToString().Split(new string[] { "," }, StringSplitOptions.None);
            y = substringsLa[0] + "." + substringsLa[1];
            x = substringsLo[0] + "." + substringsLo[1];
        }
        else
        {
            y = la.ToString();
            x = lo.ToString();
        }
        string query = $@"
                            DECLARE @sr INT = 4326;
                            DECLARE @source GEOGRAPHY;
                            SET @source = geography::Point({y}, {x}, @sr);

                            SELECT TOP (15)
                                   Filialen_nach_Distanz.[Vertriebslinie] AS {nameof(CustomerBranch.Vertriebslinie)}
                                   ,Filialen_nach_Distanz.[Filial_ID] AS {nameof(CustomerBranch.Filial_ID)}
                                   ,Filialen_nach_Distanz.[Filial_Nr] AS {nameof(CustomerBranch.Filial_Nr)}
                                   ,Filialen_nach_Distanz.[Filialname] AS {nameof(CustomerBranch.Filialname)}
                                   ,Filialen_nach_Distanz.[Straße] AS {nameof(CustomerBranch.Straße)}
                                   ,Filialen_nach_Distanz.[PLZ] AS {nameof(CustomerBranch.PLZ)}
                                   ,Filialen_nach_Distanz.[Ort] AS {nameof(CustomerBranch.ORT)}
                                   ,Filialen_nach_Distanz.[X_WGS84] AS {nameof(CustomerBranch.X_WGS84)}
                                   ,Filialen_nach_Distanz.[Y_WGS84] AS {nameof(CustomerBranch.Y_WGS84)}
                                   ,Filialen_nach_Distanz.Entfernung_in_km AS {nameof(CustomerBranch.Entfernung_in_km)}
                            FROM
                            (
                                SELECT
                                       *,
                                       ROUND(@source.STDistance(geography::Point(Y_WGS84, X_WGS84, @sr)) / 1000, 1) AS Entfernung_in_km
                                FROM [MEDIACENTER].[dbo].[Filialen_und_Konzerne]
                                WHERE Kunden_ID = {customerId}
                                      AND Y_WGS84 IS NOT NULL
                                      AND Y_WGS84 > 0
                                      AND Stop = 0
                                      AND Filial_Nr <> '{customerBranch.Filial_Nr}'
                            ) Filialen_nach_Distanz
                            ORDER BY Filialen_nach_Distanz.Entfernung_in_km;";

        var results = DbConnection.Query<CustomerBranch>(query).ToList();
        if (results != null)
        {
            return results;
        }
        return new List<CustomerBranch>();
    }
}