using ArcGisPlannerToolbox.WPF.Constants;
using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGisPlannerToolbox.Persistence.Repositories
{
    /// <summary>
    /// This is the repository for interaction with planning data.
    /// </summary>
    public class PlanningRepository : Repository<PlanningData>, IPlanningRepository
    {
        private readonly IDbContext _dbContext;
        private IDbTransaction _dbTransaction;
        private int _formerPlanningNumber;
        private bool _formerPlanningNumberSet = false;

        public int FormerPlanningNumber
        {
            get { return _formerPlanningNumber; }
            set
            {
                _formerPlanningNumber = value;
                _formerPlanningNumberSet = true;
            }
        }


        public PlanningRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.GEBIETSASSISTENT)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// It returns a list of former plannings for a given customer and planning type
        /// </summary>
        /// <param name="customerId">int</param>
        /// <param name="planningType">"Werbegebietsplanung"</param>
        /// <returns>
        /// A list of PlanningData objects.
        /// </returns>
        public List<PlanningData> GetFormerPlannings(int customerId, string planningType)
        {
            string sql = $@"
                            SELECT t.Planungs_Nr AS {nameof(PlanningData.Planungs_Nr)},
                                   t.Konzept_Nr AS {nameof(PlanningData.Konzept_Nr)},
                                   t.Analyse_Id AS {nameof(PlanningData.Analyse_Id)},
                                   t.Kunden_ID AS {nameof(PlanningData.Kunden_ID)},
                                   t.Kunden_Name AS {nameof(PlanningData.Kunden_Name)},
                                   t.Planungstyp AS {nameof(PlanningData.Planungstyp)},
                                   sum(t.HH_Brutto) AS {nameof(PlanningData.HH_Brutto)},
                                   sum(t.HH_Netto) AS {nameof(PlanningData.HH_Netto)},
                                   sum(t.Auflage) AS {nameof(PlanningData.Auflage)},
                                   Max(t.Planungsende) AS {nameof(PlanningData.Planungsende)},
                                   t.Planung_von  AS {nameof(PlanningData.Planung_von)}
                            FROM (
                            SELECT [Planungs_Nr],
                                    [Konzept_Nr],
                                    [Analyse_Id],
                                    [Kunden_ID],
                                    [Kunden_Name],
                                    [Planungstyp],
                                    SUM(HH_Brutto) HH_Brutto,
                                    SUM(HH_Netto) HH_Netto,
                                    SUM(Auflage) Auflage,
                                    MIN(Planungsbeginn) Planungsbeginn,
                                    [Planungsende],
                                    [Planung_von]
                            FROM [GEBIETSASSISTENT].[dbo].[Werbegebietsplanung]
                            WHERE Kunden_ID = {customerId}
                                AND [Planungstyp] = 'Großraumplanung'
                                AND [Planungsende] IS NOT NULL
                            GROUP BY Planungs_Nr,
                                    Konzept_Nr,
                                    Analyse_Id,
                                    Kunden_ID,
                                    Kunden_Name,
                                    Planungstyp,
                                    Planungsende,
                                    Planung_von
                            ) t
                            GROUP BY t.Planungs_Nr,
                                     t.Konzept_Nr,
                                     t.Analyse_Id,
                                     t.Kunden_ID,
                                     t.Kunden_Name,
                                     t.Planungstyp,
                                     t.Planung_von
                            ";
            var result = DbConnection.Query<PlanningData>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<PlanningData>();
        }

        /// <summary>
        /// It deletes the former branch selection of the user
        /// </summary>
        /// <param name="customerId">int</param>
        public void DeleteFormerBranchSelection(int customerId)
        {
            string deleteTableSql = $"Delete from dbo.Filialnummern_Teilnahme_Tertialplanung Where Nutzer = '{Environment.UserName}' and Kunden_ID = {customerId}";
            DbConnection.Execute(deleteTableSql);
        }

        public async Task DeleteFormerBranchSelectionAsync(int customerId)
        {
            string deleteTableSql = $"Delete from dbo.Filialnummern_Teilnahme_Tertialplanung Where Nutzer = '{Environment.UserName}' and Kunden_ID = {customerId}";
            await DbConnection.ExecuteAsync(deleteTableSql);
        }

        /// <summary>
        /// It deletes a branch from the table "Filialnummern_Teilnahme_Tertialplanung" where the user
        /// name is the current user name, the customer id is the customer id and the branch number is
        /// the branch number
        /// </summary>
        /// <param name="customerId">int</param>
        /// <param name="branchNumber">"0012"</param>
        public void DeleteBranchFromFormerBranchSelection(int customerId, string branchNumber)
        {
            string deleteTableSql = $"Delete from dbo.Filialnummern_Teilnahme_Tertialplanung Where Nutzer = '{Environment.UserName}' and Kunden_ID = {customerId} and Filial_Nr = '{branchNumber}'";
            DbConnection.Execute(deleteTableSql);
        }

        public async Task DeleteBranchFromFormerBranchSelectionAsync(int customerId, string branchNumber)
        {
            string deleteTableSql = $"Delete from dbo.Filialnummern_Teilnahme_Tertialplanung Where Nutzer = '{Environment.UserName}' and Kunden_ID = {customerId} and Filial_Nr = '{branchNumber}'";
            await DbConnection.ExecuteAsync(deleteTableSql);
        }


        /// <summary>
        /// It uploads data to a database
        /// </summary>
        /// <param name="customerId">int</param>
        /// <param name="CustomerBranch"></param>
        /// <returns>
        /// The return type is void.
        /// </returns>
        public async Task ExecuteUploadBranchDataToDatabase(int customerId, CustomerBranch branch)
        {
            if (!BranchNumberFormatValidated(branch))
            {
                if (!UpdateParticipatingBranchesCanceled(branch))
                {
                    return;
                }
                else
                {
                    throw new Exception("User canceled upload");
                }
            }
            try
            {
                await DeleteBranchFromFormerBranchSelectionAsync(customerId, branch.Filial_Nr);
                string sql = $@"
	                        INSERT INTO dbo.Filialnummern_Teilnahme_Tertialplanung
	                        (
	                            Filial_Nr,
	                            Kunden_ID,
	                            Zielauflage,
                                Nutzer
	                        )
	                        VALUES
	                        (   '{branch.Filial_Nr}', -- Filial_Nr - varchar(10)
	                            {customerId}, -- Kunden_ID - int
	                            {branch.Auflage}, -- Auflage - int
                                '{Environment.UserName}'
                            )
                            ";
                await DbConnection.ExecuteAsync(sql);
            }
            catch (Exception ex)
            {
                if (!UpdateParticipatingBranchesCanceled(branch))
                {
                    return;
                }
                else
                {
                    throw new Exception("User canceled upload");
                }
            }
        }

        /// <summary>
        /// It checks if the branch number is a number
        /// </summary>
        /// <param name="CustomerBranch">is a class that contains the properties of the branch.</param>
        /// <returns>
        /// A boolean value.
        /// </returns>
        private bool BranchNumberFormatValidated(CustomerBranch branch)
        {

             if (branch.Filial_Nr.Contains("_"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// It shows a message box and returns true if the user clicks "No" and false if the user clicks
        /// "Yes"
        /// </summary>
        /// <param name="CustomerBranch">A class that contains the data of a branch.</param>
        /// <returns>
        /// The return value is a boolean.
        /// </returns>
        private bool UpdateParticipatingBranchesCanceled(CustomerBranch branch)
        {
            var dialogResult = MessageBox.Show(
                        $"Die Daten der Filiale {branch.Filial_Nr} konnten nicht eingefügt werden. Möglicherweise passt das Format nicht. Dennoch fortfahren?",
                        "Fehler beim Sichern der Filialdaten",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                        );
            if (dialogResult == MessageBoxResult.No)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// It creates a new connection to the database
        /// </summary>
        /// <param name="DbConnectionName">This is an enum that contains the name of the connection
        /// string in the web.config file.</param>
        /// <returns>
        /// A new connection to the database.
        /// </returns>
        private SqlConnection GetNewConnectionToDatbase(DbConnectionName name)
        {
            var connection = AppConfig.GetConfiguration(name.ToString());
            return (SqlConnection)_dbContext.CreateDbConnection(connection, name);
            //var container = DiContainer.GetContainer();
            //var connectionFactory = container.GetInstance<IDbConnectionFactory>();
            //return (SqlConnection)connectionFactory.CreateDbConnection(name);
        }

        /// <summary>
        /// It returns the number of branches that are to be planned
        /// </summary>
        /// <returns>
        /// The number of rows in the table.
        /// </returns>
        public async Task<int> GetBranchesToBePlannedCountAsync()
        {
            string sql = $@"SELECT COUNT(1) FROM dbo.Filialnummern_Teilnahme_Tertialplanung";

            return await DbConnection.QueryFirstOrDefaultAsync<int>(sql);
        }

        /// <summary>
        /// It returns the number of branches that are to be planned for a given customer
        /// </summary>
        /// <param name="customerId">The customer id</param>
        /// <returns>
        /// The number of branches that are to be planned.
        /// </returns>
        public int GetBranchesToBePlannedCount(int customerId)
        {
            var dbConnection = GetNewConnectionToDatbase(DbConnectionName.GEBIETSASSISTENTMonitor);
            string sql = $@"SELECT COUNT(1) FROM 
                            (
                                SELECT Filial_Nr FROM dbo.Filialnummern_Teilnahme_Tertialplanung
                                WHERE Kunden_ID = {customerId}
                                AND Nutzer = '{Environment.UserName}'
	                           --SELECT F.Filial_Nr
                               --FROM dbo.Filialnummern_Teilnahme_Tertialplanung F
                               --Left Join dbo.Filial_Gebiet_Mapping_Großraum M
                               --    ON F.Filial_Nr = M.Filial_Nr
                               --Where M.Filial_Nr is not NULL
                               --    And M.Kunden_ID = {customerId}
			                   --    AND f.Kunden_ID = {customerId}
	                           --GROUP BY F.Filial_Nr
                                ) AS tab
                            ";
            var result = dbConnection.QueryFirstOrDefault<int>(sql);
            dbConnection.Close();
            return result;
        }

        /// <summary>
        /// Returns the current planning number of the user.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public int GetCurrentPlanningNumber(DateTime startTime)
        {
            string sql = $@"
                            SELECT MAX(Planungs_Nr) FROM dbo.Werbegebietsplanung
                            ";
            return DbConnection.QueryFirstOrDefault<int>(sql);
        }

        /// <summary>
        /// It returns the number of branches that are planned by the current user
        /// </summary>
        /// <param name="DateTime">startTime</param>
        /// <returns>
        /// The number of branches that are planned by the current user.
        /// </returns>
        public async Task<int> GetCurrentlyPlannedBranchesCountAsync(DateTime startTime)
        {
            var dbConnection = GetNewConnectionToDatbase(DbConnectionName.GEBIETSASSISTENTMonitor);
            string sql = $@"
                            SELECT Count(1) FROM dbo.Werbegebietsplanung
                            WHERE Planungsbeginn > '{startTime.ToString("yyyy-MM-dd H:mm:ss")}'
	                            AND Planung_von = '{Environment.UserName}'
                            ";
            return await dbConnection.QueryFirstOrDefaultAsync<int>(sql);
        }

        /// <summary>
        /// It returns the number of branches that are currently planned by the user
        /// </summary>
        /// <param name="DateTime">startTime</param>
        /// <returns>
        /// The number of branches that are currently planned by the user.
        /// </returns>
        public int GetCurrentlyPlannedBranchesCount(DateTime startTime)
        {
            var dbConnection = GetNewConnectionToDatbase(DbConnectionName.GEBIETSASSISTENTMonitor);
            string sql = $@"
                            SELECT Count(1) FROM dbo.Werbegebietsplanung
                            WHERE Planungsbeginn > '{startTime.ToString("yyyy-MM-dd H:mm:ss")}'
	                            AND Planung_von = '{Environment.UserName}'
                            ";
            var result = dbConnection.QueryFirstOrDefault<int>(sql);
            dbConnection.Close();
            return result;
        }

        /// <summary>
        /// It returns the number of branches that have been finished by the current user since the given
        /// start time
        /// </summary>
        /// <param name="DateTime">startTime</param>
        /// <returns>
        /// The number of finished branches.
        /// </returns>
        public async Task<int> GetFinishedBranchesCountAsync(DateTime startTime)
        {
            string sql = $@"
                            SELECT COUNT(1) FROM dbo.Werbegebietsplanung
                            WHERE Planungsende IS NOT NULL
                                AND Planungsbeginn > '{startTime.ToString("yyyy-MM-dd H:mm:ss")}'
                                AND Planung_von '{Environment.UserName}'
                            ";
            return await DbConnection.QueryFirstOrDefaultAsync<int>(sql);

        }

        /// <summary>
        /// It returns the number of branches that have been finished by the current user since the
        /// given start time
        /// </summary>
        /// <param name="DateTime">startTime</param>
        /// <returns>
        /// The number of branches that have been finished since the startTime.
        /// </returns>
        public int GetFinishedBranchesCount(DateTime startTime)
        {
            var dbConnection = GetNewConnectionToDatbase(DbConnectionName.GEBIETSASSISTENTMonitor);
            string sql = $@"
                            SELECT COUNT(1) FROM dbo.Werbegebietsplanung
                            WHERE Planungsende IS NOT NULL
                                AND Planungsbeginn > '{startTime.ToString("yyyy-MM-dd H:mm:ss")}'
                                AND Planung_von = '{Environment.UserName}'
                            ";
            var result = dbConnection.QueryFirstOrDefault<int>(sql);
            dbConnection.Close();
            return result;
        }

        /// <summary>
        /// It returns the number of address circles that have been finished by the current user since
        /// the given start time
        /// </summary>
        /// <param name="startTime">Start Time</param>
        /// <returns>
        /// The number of address circles that have been finished by the user.
        /// </returns>
        public int GetFinishedAddressCirclesCount(DateTime startTime)
        {
            var dbConnection = GetNewConnectionToDatbase(DbConnectionName.GEBIETSASSISTENTMonitor);
            string sql = $@"
                            SELECT COUNT(1) FROM dbo.Werbegebietsplanung_Adresskreise
                            WHERE Planungsende IS NOT NULL
                                AND Planungsbeginn > '{startTime.ToString("yyyy-MM-dd H:mm:ss")}'
                                AND Planung_von = '{Environment.UserName}'
                            ";
            var result = dbConnection.QueryFirstOrDefault<int>(sql);
            dbConnection.Close();
            return result;
        }

        /// <summary>
        /// It executes a stored procedure in the database
        /// </summary>
        /// <param name="analysisId">The id of the analysis you want to execute the planner for</param>
        /// <param name="planningLevel">The planning level is the level of the hierarchy that you want
        /// to plan. For example, if you want to plan at the branch level, the planning level would be
        /// "Filiale".</param>
        /// <param name="mindNonParticipatingBranches">If true, the planner will only consider branches
        /// that are participating in the analysis. If false, all branches will be considered.</param>
        /// <param name="customerPercentage">The customer percentage in an area</param>
        public void ExecuteAutoPlannerStoredProcedure(int analysisId, string planningLevel, bool allowCanibalization, int customerPercentage)
        {
            var procedure = "AutoAreaPlanner";
            var values = new
            {
                Analyse_Id = analysisId,
                Planungsebene = planningLevel,
                nurTeilnehmendeFilialen = allowCanibalization ? 0 : 1,
                User = Environment.UserName,
                Kunden_Anteil = customerPercentage
            };
            base.DbConnection.Execute(procedure, values, null, commandTimeout: 0, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// It executes a stored procedure on the database
        /// </summary>
        /// <param name="analysisId">The id of the analysis</param>
        /// <param name="planningLevel">The planning level is a string that can be either "Branch" or
        /// "Area".</param>
        /// <param name="mindNonParticipatingBranches">If true, the procedure will only consider</param>
        /// <param name="customerPercentage">The customer percentage in an area</param>
        /// branches that are participating in the analysis. If false, it will consider all
        /// branches.</param>
        public async Task ExecuteAutoPlannerStoredProcedureAsync(int analysisId, string planningLevel, bool allowCanibalization, int customerPercentage)
        {
            try
            {
                var procedure = "AutoAreaPlanner";
                var values = new
                {
                    Analyse_Id = analysisId,
                    Planungsebene = planningLevel,
                    nurTeilnehmendeFilialen = allowCanibalization ? 0 : 1,
                    User = Environment.UserName,
                    Kunden_Anteil = customerPercentage
                };
                await base.DbConnection.ExecuteAsync(procedure, values, null, commandTimeout: 0, commandType: CommandType.StoredProcedure);
            }
            catch (DbException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// It executes a stored procedure with the given parameters
        /// </summary>
        /// <param name="analysisId">The id of the analysis</param>
        /// <param name="planningLevel">"Branch"</param>
        /// <param name="planninNumber">The number of the planning.</param>
        public void ExecuteAutoPlannerForNewBranchesStoredProcedure(int analysisId, string planningLevel, int planninNumber)
        {
            var procedure = "AutoAreaPlanner_Recalculate";
            var values = new
            {
                Analyse_Id = analysisId,
                Planungsebene = planningLevel,
                Planungs_Nr = planninNumber,
                User = Environment.UserName
            };
            base.DbConnection.Execute(procedure, values, null, commandTimeout: 0, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// It executes a stored procedure in a SQL Server database
        /// </summary>
        /// <param name="conceptNumber">The concept number of the concept you want to use.</param>
        /// <param name="analysisId">The id of the analysis that was created in the first step.</param>
        /// <param name="minIntersections">The minimum number of intersections that must be included in
        /// the planning.</param>
        /// <param name="maxDifference">The maximum difference between the number of intersections of
        /// the two circles.</param>
        public async Task ExecuteAddressCirclePlanner(int conceptNumber, int analysisId, int minIntersections, int maxDifference)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            var procedure = "AddressCirclePlanner";
            var values = new
            {
                Planungs_Nr = planningNumber,
                Konzept_Nr = conceptNumber,
                Analyse_Id = analysisId,
                MinÜberschneidungen = minIntersections,
                MaxAnteilsUnterschied = maxDifference,
                Benutzer = Environment.UserName
            };


            await DbConnection.ExecuteAsync(procedure, values, null, commandTimeout: 0, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// It returns the maximum planning number for the current user
        /// </summary>
        /// <param name="SqlConnection">The connection to the database.</param>
        /// <returns>
        /// The max planning number from the database.
        /// </returns>
        private int GetMaxPlanningNumber(SqlConnection dbConnection = null)
        {
            var connection = dbConnection ?? DbConnection;
            string sql = $@"
                            SELECT ISNULL(MAX(Planungs_Nr), 0) MaxPlanungs_Nr FROM dbo.Werbegebietsplanung
	                            WHERE Planung_von = '{Environment.UserName}'
                            ";

            return connection.ExecuteScalar<int>(sql);
        }

        private int GetMaxKonzeptNumberByPlanningNumber(int planningNumber, SqlConnection dbConnection = null)
        {
            var connection = dbConnection ?? DbConnection;
            string sql = $@"
                            SELECT MAX(Konzept_Nr) MaxKonzept_Nr FROM dbo.Werbegebietsplanung
	                            WHERE Planung_von = '{Environment.UserName}'
								AND Planungs_Nr = {planningNumber}
                            ";

            return connection.ExecuteScalar<int>(sql);
        }

        /// <summary>
        /// It gets the planning data from the database
        /// </summary>
        /// <returns>
        /// A list of PlanningData objects.
        /// </returns>
        public List<PlanningData> GetPlanningData()
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT Filial_Nr AS {nameof(PlanningData.Filial_Nr)},
                                   Planungstyp AS {nameof(PlanningData.Planungstyp)},
                                   Zielauflage1 AS {nameof(PlanningData.Zielauflage1)},
                                   Zielauflage2 AS {nameof(PlanningData.Zielauflage2)},
                                   Zielauflage3 AS {nameof(PlanningData.Zielauflage3)},
                                   Mindestauflage AS {nameof(PlanningData.Mindestauflage)},
                                   Maximalauflage AS {nameof(PlanningData.Maximalauflage)},
                                   Stand AS {nameof(PlanningData.Stand)},
                                   HH_Brutto AS {nameof(PlanningData.HH_Brutto)},
                                   HH_Netto AS {nameof(PlanningData.HH_Netto)},
                                   Auflage AS {nameof(PlanningData.Auflage)},
                                   Planungsbeginn AS {nameof(PlanningData.Planungsbeginn)},
                                   Planungsende AS {nameof(PlanningData.Planungsende)},
                                   Planung_von AS {nameof(PlanningData.Planung_von)}
                            FROM dbo.Werbegebietsplanung
                            WHERE Planungs_Nr = {planningNumber}
                            ";

            var result = DbConnection.Query<PlanningData>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<PlanningData>();
        }

        public List<PlanningData> GetPlanningData(int planningNumber)
        {
            string sql = $@"
                            SELECT Filial_Nr AS {nameof(PlanningData.Filial_Nr)},
                                   Planungstyp AS {nameof(PlanningData.Planungstyp)},
                                   Zielauflage1 AS {nameof(PlanningData.Zielauflage1)},
                                   Zielauflage2 AS {nameof(PlanningData.Zielauflage2)},
                                   Zielauflage3 AS {nameof(PlanningData.Zielauflage3)},
                                   Mindestauflage AS {nameof(PlanningData.Mindestauflage)},
                                   Maximalauflage AS {nameof(PlanningData.Maximalauflage)},
                                   Stand AS {nameof(PlanningData.Stand)},
                                   HH_Brutto AS {nameof(PlanningData.HH_Brutto)},
                                   HH_Netto AS {nameof(PlanningData.HH_Netto)},
                                   Auflage AS {nameof(PlanningData.Auflage)},
                                   Planungsbeginn AS {nameof(PlanningData.Planungsbeginn)},
                                   Planungsende AS {nameof(PlanningData.Planungsende)},
                                   Planung_von AS {nameof(PlanningData.Planung_von)}
                            FROM dbo.Werbegebietsplanung
                            WHERE Planungs_Nr = {planningNumber}
                            ";

            var result = DbConnection.Query<PlanningData>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<PlanningData>();
        }


        /// <summary>
        /// It returns a list of PlanningData objects, which are created from the data in the database
        /// </summary>
        /// <param name="planningNumber">int</param>
        /// <returns>
        /// A list of PlanningData objects.
        /// </returns>
        public List<PlanningData> GetPlanningDataByPlanningNumberAndConceptNumber(int planningNumber, int conceptNumber)
        {
            string sql = $@"
                            SELECT Filial_Nr AS {nameof(PlanningData.Filial_Nr)},
                                   Planungstyp AS {nameof(PlanningData.Planungstyp)},
                                   Zielauflage1 AS {nameof(PlanningData.Zielauflage1)},
                                   Zielauflage2 AS {nameof(PlanningData.Zielauflage2)},
                                   Zielauflage3 AS {nameof(PlanningData.Zielauflage3)},
                                   Mindestauflage AS {nameof(PlanningData.Mindestauflage)},
                                   Maximalauflage AS {nameof(PlanningData.Maximalauflage)},
                                   Stand AS {nameof(PlanningData.Stand)},
                                   HH_Brutto AS {nameof(PlanningData.HH_Brutto)},
                                   HH_Netto AS {nameof(PlanningData.HH_Netto)},
                                   Auflage AS {nameof(PlanningData.Auflage)},
                                   Planungsbeginn AS {nameof(PlanningData.Planungsbeginn)},
                                   Planungsende AS {nameof(PlanningData.Planungsende)},
                                   Planung_von AS {nameof(PlanningData.Planung_von)}
                            FROM dbo.Werbegebietsplanung
                            WHERE Planungs_Nr = {planningNumber}
                                AND Konzept_Nr = {conceptNumber}
                            ";

            var result = DbConnection.Query<PlanningData>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<PlanningData>();
        }

        /// <summary>
        /// It returns a list of PlanningDataArea objects, which are basically a list of polygons
        /// </summary>
        /// <returns>
        /// A list of PlanningDataArea objects.
        /// </returns>
        public List<PlanningDataArea> GetPlanningDataFromAllBranches()
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT	wg.Planungs_Nr As {nameof(PlanningDataArea.Planungs_Nr)},
	                                wg.Planungs_ID As {nameof(PlanningDataArea.Planungs_ID)},
                                    wg.Filial_Nr As {nameof(PlanningDataArea.Filial_Nr)},
                                    wg.Geokey As {nameof(PlanningDataArea.Geokey)},
                                    gg.Geokey_Name As {nameof(PlanningDataArea.Geokey_Name)},
                                    gg.HH_Brutto As {nameof(PlanningDataArea.HH_Brutto)},
                                    gg.HH_Netto As {nameof(PlanningDataArea.HH_Netto)},
                                    wg.geom.STAsBinary() As {nameof(PlanningDataArea.Geom)}
                            FROM dbo.Werbegebietsplanung_Geometrien wg
                            Left Join dbo.Geoservices_Geometries gg
                                ON gg.Geokey = wg.Geokey
                            WHERE Planungs_Nr = {planningNumber}
                            ";

            var result = DbConnection.Query<PlanningDataArea>(sql).ToList();
            if (result.Any())
            {
                return result;
            }
            return new List<PlanningDataArea>();
        }

        /// <summary>
        /// It returns a list of PlanningDataArea objects that are overlapping with the branch number
        /// and analysis id provided
        /// </summary>
        /// <param name="branchNumber">string</param>
        /// <param name="analysisId">int</param>
        /// <returns>
        /// A list of PlanningDataArea objects.
        /// </returns>
        public List<PlanningDataArea> GetOverlappingPlanningDataByBranchNumber(string branchNumber, int analysisId)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT wg.Planungs_Nr As {nameof(PlanningDataArea.Planungs_Nr)},
	                                wg.Planungs_ID As {nameof(PlanningDataArea.Planungs_ID)},
                                    wg.Filial_Nr As {nameof(PlanningDataArea.Filial_Nr)},
                                    wg.Geokey As {nameof(PlanningDataArea.Geokey)},
                                    gg.Geokey_Name As {nameof(PlanningDataArea.Geokey_Name)},
                                    gg.HH_Brutto As {nameof(PlanningDataArea.HH_Brutto)},
                                    gg.HH_Netto As {nameof(PlanningDataArea.HH_Netto)},
                                    wg.geom.STAsBinary() As {nameof(PlanningDataArea.Geom)}
                            FROM dbo.Werbegebietsplanung_Geometrien wg
                            LEFT JOIN dbo.Geoservices_Geometries gg
	                            ON gg.Geokey = wg.Geokey
                            Inner JOIN (
							SELECT sub.Filial_Nr, sub.Analyse_Id FROM (
									SELECT M.Filial_Nr, M.Analyse_Id
									FROM dbo.Werbegebietsplanung_Geometrien W
									LEFT JOIN dbo.Filial_Gebiet_Mapping_Großraum M
										ON M.Geokey = W.Geokey
									Left JOIN dbo.Werbegebietsplanung T
										ON T.Filial_Nr = M.Filial_Nr
									WHERE W.Planungs_Nr = {planningNumber}
										AND M.Filial_Nr = '{branchNumber}'
										AND t.Filial_Nr IS NOT NULL
									UNION ALL	
									SELECT G.Filial_Nr, M.Analyse_Id
									FROM dbo.Filial_Gebiet_Mapping_Großraum M
									LEFT JOIN dbo.Werbegebietsplanung_Geometrien G
										ON G.Geokey = M.Geokey
									WHERE M.Filial_Nr = '{branchNumber}'
										AND G.Planungs_Nr = {planningNumber}
								)sub 
								GROUP BY sub.Filial_Nr, sub.Analyse_Id
                            ) tab
	                            ON tab.Filial_Nr = wg.Filial_Nr
                            WHERE wg.Planungs_Nr = {planningNumber}
                                AND wg.Aktiv = 1
                                AND tab.Analyse_Id = {analysisId}
                            ";

            var result = DbConnection.Query<PlanningDataArea>(sql).ToList();
            if (result.Any())
            {
                return result;
            }
            return new List<PlanningDataArea>();

        }

        /// <summary>
        /// It returns a list of PlanningDataArea objects, which are basically just a list of polygons
        /// </summary>
        /// <param name="branchNumber">"00100"</param>
        /// <returns>
        /// A list of PlanningDataArea objects.
        /// </returns>
        public List<PlanningDataArea> GetPlanningDataAreasByBranchNumber(string branchNumber)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT	wg.Filial_Nr As {nameof(PlanningDataArea.Filial_Nr)},
                                    wg.Geokey As {nameof(PlanningDataArea.Geokey)},
                                    gg.HH_Brutto As {nameof(PlanningDataArea.HH_Brutto)},
                                    gg.HH_Netto As {nameof(PlanningDataArea.HH_Netto)},
                                    wg.geom.STAsBinary() As {nameof(PlanningDataArea.Geom)}
                            FROM dbo.Werbegebietsplanung_Geometrien wg
                            Left Join dbo.Geoservices_Geometries gg
                                ON gg.Geokey = wg.Geokey
                            WHERE Planungs_Nr = {planningNumber}
                                AND Filial_Nr = '{branchNumber}'
                                AND wg.Aktiv = 1
                            ";

            var result = DbConnection.Query<PlanningDataArea>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<PlanningDataArea>();
        }

        public void ReleasePredefinedBranchNumbers(List<string> releasables, int customerId)
        {
            foreach (var releasable in releasables)
            {
                string sql = $@"
                            DELETE FROM dbo.Werbegebietsplanung_vordefinierte_Filialgebiete
                            WHERE Filial_Nr = '{releasable}'
	                            AND Kunden_ID = {customerId}
                            ";

                DbConnection.ExecuteScalar(sql);
            }

        }

        public List<PredefinedBranch> GetPredefinedBranches(int customerId)
        {
            string sql = $@"
                            SELECT v.Filial_ID as {nameof(PredefinedBranch.Filial_Id)},
                                v.Filial_Nr as {nameof(PredefinedBranch.Filial_Nr)}, 
                                IIF(m.Anzahl_Ergebnisse > 0, 1, 0) as {nameof(PredefinedBranch.Kundendaten_verfügbar)}, 
                                w.Auflage_Filiale as {nameof(PredefinedBranch.Auflage)}
                            FROM dbo.Werbegebietsplanung_vordefinierte_Filialgebiete v
                            LEFT JOIN 
                            (
	                            SELECT t.Analyse_ID,
                                       t.Kunde_ID
	                            FROM (
		                            SELECT Analyse_ID, Kunde_ID,  ROW_NUMBER() OVER (partition BY Kunde_Id ORDER BY Analyse_ID DESC) Rang 
		                            FROM dbo.vw_Analysedaten_Gebietsassistent_SQL
	                            )t
	                            WHERE t.Rang = 1
                            ) A
	                            ON v.Kunden_ID = a.Kunde_ID
                            LEFT JOIN (
	                            SELECT COUNT(1) Anzahl_Ergebnisse, Filial_Nr, Analyse_Id FROM dbo.Filial_Gebiet_Mapping_Großraum
	                            GROUP BY Filial_Nr, Analyse_Id
                            ) m
	                            ON m.Analyse_Id = A.Analyse_ID
		                            AND v.Filial_Nr = m.Filial_Nr
                            LEFT JOIN dbo.Werbegebietsplanung_Resultset w
	                            ON v.Planungs_Nr = w.Planungs_Nr
		                            AND v.Konzept_Nr + 1 = w.Konzept_Nr
		                            AND v.Filial_Nr = w.Filial_Nr
                            WHERE v.Kunden_ID = {customerId}
                            ";

            var result = DbConnection.Query<PredefinedBranch>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<PredefinedBranch>();
        }

        public List<PlanningDataArea> GetPredefinedArea(string branchNumber)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT P.Geokey as {nameof(PlanningDataArea.Geokey)},
		                            G.HH_Brutto as {nameof(PlanningDataArea.HH_Brutto)},
		                            G.HH_Netto as {nameof(PlanningDataArea.HH_Netto)},
		                            r.Auflage_PLZ as {nameof(PlanningDataArea.Auflage)}
                            FROM dbo.Werbegebietsplanung_Geometrien P
                            LEFT JOIN dbo.Geoservices_Geometries G
	                            ON G.Geokey = P.Geokey
                            LEFT JOIN dbo.Werbegebietsplanung_Resultset r
	                            ON p.Geokey = r.PLZ
		                            AND p.Planungs_Nr = r.Planungs_Nr
                            WHERE P.Planungs_Nr = {planningNumber}
	                            AND P.Konzept_Nr = 0
	                            AND P.Filial_Nr = '{branchNumber}'
                            ";

            var result = DbConnection.Query<PlanningDataArea>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<PlanningDataArea>();
        }

        /// <summary>
        /// It executes a stored procedure in the database
        /// </summary>
        /// <param name="branchNumber">string</param>
        /// <param name="analysisId">int</param>
        /// <param name="customerId">string</param>
        public void ExecuteStoredProcedurePlanningSelectionTable(string branchNumber, int analysisId, int customerId)
        {
            var procedure = "Prepare_Planning_Selection_Table_new";
            var values = new { Filial_Nr = branchNumber, Analyse_Id = analysisId, Kunden_Id = customerId };
            base.DbConnection.Execute(procedure, values, null, commandTimeout: 0, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// It returns a list of PlanningDataArea objects, which are filled with data from a SQL Server
        /// database
        /// </summary>
        /// <returns>
        /// A list of PlanningDataArea objects.
        /// </returns>
        public List<PlanningDataArea> GetGeokysFromTempZipCodeTable()
        {
            string sql = $@"
                            SELECT P.Geokey As {nameof(PlanningDataArea.Geokey)},
                                    G.HH_Brutto As {nameof(PlanningDataArea.HH_Brutto)},
                                    G.HH_Netto As {nameof(PlanningDataArea.HH_Netto)},
                                    G.geom.STAsBinary() As {nameof(PlanningDataArea.Geom)}
	                            FROM [TMP].[PLZ_Selekt] P
                            LEFT JOIN dbo.Geoservices_Geometries G
	                            ON G.Geokey = P.Geokey
                            WHERE Benutzername = SUSER_NAME()
                            ";
            var result = DbConnection.Query<PlanningDataArea>(sql).ToList();
            if (result.Any())
            {
                return result;
            }
            return new List<PlanningDataArea>();
        }

        public void SavePredefinedBranches(List<string> branchNumbers, int customerId)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            foreach (var branchNumber in branchNumbers)
            {
                try
                {
                    string sql = $@"
                                    INSERT INTO dbo.Werbegebietsplanung_vordefinierte_Filialgebiete
                                    (
                                        Filial_ID,
                                        Filial_Nr,
                                        Planungs_Nr,
                                        Konzept_Nr,
                                        Kunden_Id
                                    )
                                    SELECT Filial_ID,
                                        '{branchNumber}',   -- Filial_Nr - varchar(255)
                                        {planningNumber}, -- Planungs_Nr - int
                                        0,  -- Konzept_Nr - int
                                        {customerId}
                                    FROM dbo.SuperOffice_Filialen_und_Konzerne
                                    WHERE 1=1
	                                    AND Filial_Nr = '{branchNumber}'
	                                    AND Kunden_ID = {customerId}";
                    DbConnection.ExecuteScalar(sql);
                }
                catch (SqlException e)
                {
                    string sql = $@"
                                    UPDATE v
                                    SET v.Planungs_Nr = {planningNumber}
                                    FROM dbo.Werbegebietsplanung_vordefinierte_Filialgebiete v
                                    WHERE v.Filial_Nr = '{branchNumber}'
                                        AND Kunden_ID = {customerId}
                                    ";
                    DbConnection.ExecuteScalar(sql);
                }
            }

        }

        /// <summary>
        /// It gets the address circle of a branch by the branch number
        /// </summary>
        /// <param name="branchNumber">"0012"</param>
        /// <returns>
        /// The first string from the query.
        /// </returns>
        public string GetAddressCircleByBranchNumber(string branchNumber)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT AK FROM dbo.Werbegebietsplanung_Adresskreise
                            WHERE Planungs_Nr = {planningNumber}
                                AND Filial_Nr = '{branchNumber}'
                            ";

            return DbConnection.QueryFirst<string>(sql);
        }

        /// <summary>
        /// It checks if a customer has a certain address circle
        /// </summary>
        /// <param name="customerId">int</param>
        /// <returns>
        /// The result is a boolean value.
        /// </returns>
        public bool CheckAddressCircleNeed(int customerId)
        {
            string sql = $@"
                            SELECT Adresskreise FROM dbo.Kunden_Adresskreis_Info
                            WHERE Kunden_ID = {customerId}
                            ";
            var result = DbConnection.Query<int>(sql).FirstOrDefault();
            if (result > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// It returns a list of AddressCircles from the database
        /// </summary>
        /// <returns>
        /// A list of AddressCircles.
        /// </returns>
        public List<AddressCircle> GetAllAddressCircles()
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT ID AS {nameof(AddressCircle.Id)}, 
                                AK AS {nameof(AddressCircle.AK)}, 
                                Planungs_Nr AS {nameof(AddressCircle.Planungs_Nr)}
                            FROM dbo.Werbegebietsplanung_Adresskreise
                            WHERE Planungs_Nr = {planningNumber}
                                AND Planung_von = '{Environment.UserName}'
                            GROUP BY Id, Ak, Planungs_Nr
                            ";

            var result = DbConnection.Query<AddressCircle>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<AddressCircle>();
        }

        /// <summary>
        /// It executes a stored procedure in a SQL Server database
        /// </summary>
        public void ExecutePrepareAddressCircleAreas()
        {
            var dbConnection = GetNewConnectionToDatbase(DbConnectionName.GEBIETSASSISTENTMonitor);
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber(dbConnection);
            var procedure = "Prepare_Address_Circle_Areas";
            var values = new
            {
                Planungs_Nr = planningNumber,
                Benutzer = Environment.UserName
            };
            dbConnection.Execute(procedure, values, null, commandTimeout: 0, commandType: CommandType.StoredProcedure);
            dbConnection.Close();
        }

        /// <summary>
        /// It gets the max planning number from the database and then queries the database for the
        /// address circle areas
        /// </summary>
        /// <returns>
        /// A list of AddressCircleArea objects.
        /// </returns>
        public List<AddressCircleArea> GetAddressCircleAreas()
        {
            var dbConnection = GetNewConnectionToDatbase(DbConnectionName.GEBIETSASSISTENTMonitor);
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT AK AS {nameof(AddressCircleArea.AK)}, 
                                    Filial_Nr AS {nameof(AddressCircleArea.Filial_Nr)}, 
                                    Geom.STAsBinary() AS {nameof(AddressCircleArea.Geom)}
                            FROM dbo.Werbegebietsplanung_Adresskreise_Geometrien
                            ";

            var result = dbConnection.Query<AddressCircleArea>(sql, commandTimeout: 0).ToList();
            dbConnection.Close();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<AddressCircleArea>();
        }

        /// <summary>
        /// It returns a list of branches that are not in the address circle of the given branches
        /// </summary>
        /// <param name="branchNumbers">List of branch numbers</param>
        /// <param name="addressCircle">"A"</param>
        /// <returns>
        /// A list of strings.
        /// </returns>
        public List<string> GetRelevantBranchesExcludedFromAddressCircle(List<string> branchNumbers, string addressCircle)
        {
            string branchesListString = String.Join("','", branchNumbers);
            string sql = $@"
                            SELECT D.Nachbarfilal_Nr FROM dbo.Werbegebietsplanung_Adresskreise_Details D
                            LEFT JOIN dbo.Werbegebietsplanung_Adresskreise A
	                            ON A.Filial_Nr = D.Hauptfilial_Nr
                            WHERE D.Hauptfilial_Nr IN ('{branchesListString}')
	                            AND D.Nachbarfilal_Nr NOT IN ('{branchesListString}')
	                            AND A. Ak <> '{addressCircle}'
                            ";

            var result = DbConnection.Query<string>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<string>();
        }

        /// <summary>
        /// It returns a list of AddressCircles from the database, where the planning number is equal to
        /// the planning number passed in as a parameter
        /// </summary>
        /// <param name="planningNumber">int</param>
        /// <returns>
        /// A list of AddressCircles.
        /// </returns>
        public List<AddressCircle> GetAddressCirclesByPlanningNumber(int planningNumber)
        {
            string sql = $@"
                            SELECT ID AS {nameof(AddressCircle.Id)}, 
                                AK AS {nameof(AddressCircle.AK)}, 
                                Planungs_Nr AS {nameof(AddressCircle.Planungs_Nr)}
                            FROM dbo.Werbegebietsplanung_Adresskreise
                            WHERE Planungs_Nr = {planningNumber}
                                AND Planung_von = '{Environment.UserName}'
                            GROUP BY Id, Ak, Planungs_Nr
                            ";

            var result = DbConnection.Query<AddressCircle>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<AddressCircle>();
        }

        /// <summary>
        /// It returns a list of AddressCicleDetails objects, which are populated with data from a SQL
        /// Server database
        /// </summary>
        /// <param name="branchNumber">The branch number of the branch you want to get the details
        /// for.</param>
        /// <returns>
        /// A list of AddressCicleDetails objects.
        /// </returns>
        public List<AddressCicleDetails> GetAddressCircleDetailsByBranchNumber(string branchNumber)
        {
            string sql = $@"
                            SELECT Hauptfilial_Nr as {nameof(AddressCicleDetails.Hauptfiliale)},
                                   Nachbarfilal_Nr as {nameof(AddressCicleDetails.Nachbarfiliale)},
                                   Überschneidungen as {nameof(AddressCicleDetails.Überschneidungen)},
                                   Anteil_Gesamt as {nameof(AddressCicleDetails.Anteil_Gesamt)},
                                   Mittler_Anteil as {nameof(AddressCicleDetails.Mittlerer_Anteil)} 
                            FROM dbo.Werbegebietsplanung_Adresskreise_Details
                            WHERE Hauptfilial_Nr = '{branchNumber}'
                            ";

            var result = DbConnection.Query<AddressCicleDetails>(sql).ToList();
            if (result.Count > 0)
            {
                return result;
            }
            return new List<AddressCicleDetails>();
        }

        /// <summary>
        /// It returns a list of PlanningDataArea objects, which are basically just a list of geometries
        /// and some metadata
        /// </summary>
        /// <param name="geokeys">List of strings</param>
        /// <returns>
        /// A list of PlanningDataArea objects.
        /// </returns>
        public List<PlanningDataArea> GetCurrentOwnersOfGeokeys(List<string> geokeys)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            string sql = $@"
                            SELECT wg.Planungs_Nr As {nameof(PlanningDataArea.Planungs_Nr)},
	                                wg.Planungs_ID As {nameof(PlanningDataArea.Planungs_ID)},
                                    wg.Filial_Nr As {nameof(PlanningDataArea.Filial_Nr)},
                                    wg.Geokey As {nameof(PlanningDataArea.Geokey)},
                                    gg.Geokey_Name As {nameof(PlanningDataArea.Geokey_Name)},
                                    gg.HH_Brutto As {nameof(PlanningDataArea.HH_Brutto)},
                                    gg.HH_Netto As {nameof(PlanningDataArea.HH_Netto)},
                                    wg.geom.STAsBinary() As {nameof(PlanningDataArea.Geom)}
                            FROM dbo.Werbegebietsplanung_Geometrien wg
                            LEFT JOIN dbo.Geoservices_Geometries gg
	                            ON gg.Geokey = wg.Geokey
                            WHERE wg.Geokey IN ('{String.Join("', '", geokeys)}')
                                and wg.Planungs_Nr = {planningNumber}
                                and wg.Aktiv = 1
                            ";

            var result = DbConnection.Query<PlanningDataArea>(sql).ToList();
            if (result.Any())
            {
                return result;
            }
            return new List<PlanningDataArea>();
        }

        /// <summary>
        /// It takes a list of areas and moves them to a different branch
        /// </summary>
        /// <param name="currentOwners">A list of PlanningDataArea objects.</param>
        /// <param name="newBranchNumber">The branch number of the new owner</param>
        public void MoveAreaToDifferentBranch(List<PlanningDataArea> currentOwners, string newBranchNumber)
        {
            if (currentOwners.Any())
            {
                int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
                foreach (var owner in currentOwners.Distinct())
                {
                    RemoveGeokey(planningNumber, owner.Geokey, owner.Filial_Nr);
                    try
                    {
                        AddGeokey(planningNumber, newBranchNumber, owner.Geokey);
                    }
                    catch (Exception)
                    {
                        ReactivateGeokey(planningNumber, newBranchNumber, owner.Geokey);
                    }
                    UpdatePlanningData(owner.Filial_Nr, planningNumber);
                }
                UpdatePlanningData(newBranchNumber, planningNumber);
            }
        }

        /// <summary>
        /// It adds a list of geokeys to a branch
        /// </summary>
        /// <param name="geokeys">List of geokeys to add to the branch</param>
        /// <param name="branchNumber">string</param>
        public void AddUnselectedAreaToBranch(List<string> geokeys, string branchNumber)
        {
            if (geokeys.Any())
            {
                int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
                foreach (var geokey in geokeys.Distinct())
                {
                    try
                    {
                        AddGeokey(planningNumber, branchNumber, geokey);
                    }
                    catch (Exception)
                    {
                        ReactivateGeokey(planningNumber, branchNumber, geokey);
                    }
                }
                UpdatePlanningData(branchNumber, planningNumber);
            }
        }

        /// <summary>
        /// It takes a planning number, a branch number and a geokey and inserts it into the database
        /// </summary>
        /// <param name="planningNumber">int</param>
        /// <param name="branchNumber">"12345"</param>
        /// <param
        /// name="geokey">"The geokey like zip code or micro zipcode. </param>
        private void AddGeokey(int planningNumber, string branchNumber, string geokey)
        {
            try
            {
                _dbTransaction = CreateTransaction();
                string sql = $@"
                                DECLARE @Planungs_Id AS INT
                                DECLARE @geom AS GEOGRAPHY

                                SELECT @Planungs_Id = Planungs_ID
                                FROM dbo.Werbegebietsplanung
                                WHERE Filial_Nr = '{branchNumber}'
                                AND Planungs_Nr = {planningNumber}

                                SELECT @geom = geom FROM dbo.Geoservices_Geometries
                                WHERE Geokey = '{geokey}'

                                INSERT INTO dbo.Werbegebietsplanung_Details
                                (
                                    Planungs_ID,
                                    Planungs_Nr,
                                    Konzept_Nr,
                                    Geokey
                                )
                                VALUES
                                (   @Planungs_Id,    -- Planungs_ID - int
                                    {planningNumber},    -- Planungs_Nr - int
                                    0,    -- Konzept_Nr - int
                                    '{geokey}'   -- Geokey - varchar(14)
                                    )
                                
                                INSERT INTO dbo.Werbegebietsplanung_Geometrien
                                (
                                    Planungs_ID,
                                    Planungs_Nr,
                                    Konzept_Nr,
                                    Geokey,
                                    geom,
                                    Filial_Nr,
                                    Aktiv,
                                    Geändert_von
                                )
                                VALUES
                                (	@Planungs_Id
                                    ,{planningNumber}
                                    ,0
	                                ,'{geokey}'
	                                ,@geom
	                                ,'{branchNumber}'
	                                ,1
	                                ,'{Environment.UserName}'
                                )
                                
                                ";

                DbConnection.ExecuteScalar(sql, transaction: _dbTransaction);
                _dbTransaction.Commit();
            }
            catch (Exception e)
            {
                RollBackTransaction();
                throw new Exception("Der Eintrag existiert Bereits. Dieser kann jedoch gupdated werden.", e);
            }
        }

        /// <summary>
        /// It reactivates a geokey in the database
        /// </summary>
        /// <param name="planningNumber">int</param>
        /// <param name="branchNumber">"00100"</param>
        /// <param name="geokey">a string of numbers and letters</param>
        private void ReactivateGeokey(int planningNumber, string branchNumber, string geokey)
        {
            string sql = $@"
                            UPDATE dbo.Werbegebietsplanung_Geometrien
                            SET Aktiv = 1,
                                Geändert_von = '{Environment.UserName}'
                            WHERE Planungs_Nr = {planningNumber}
	                            AND Filial_Nr = '{branchNumber}'
	                            AND Geokey = '{geokey}'
                            ";
            DbConnection.ExecuteScalar(sql);
        }

        /// <summary>
        /// It removes the selected area from the branch
        /// </summary>
        /// <param name="geokeys">a list of geokeys that are selected on the map</param>
        /// <param name="branchNumber">The branch number of the branch that is currently logged
        /// in.</param>
        public void RemoveSelectedAreaFromBranch(List<string> geokeys, string branchNumber)
        {
            if (geokeys.Any())
            {
                int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
                foreach (var geokey in geokeys.Distinct())
                {
                    RemoveGeokey(planningNumber, geokey, branchNumber);
                    string originalOwner = GetOriginalGeokeyOwner(geokey);
                    if (originalOwner != branchNumber)
                    {
                        ResetToOriginalState(planningNumber, geokey);
                    }
                }
                UpdatePlanningData(branchNumber, planningNumber);
            }
        }

        /// <summary>
        /// It returns the original owner of a geokey
        /// </summary>
        /// <param name="geokey">a string that is a unique identifier for a polygon</param>
        /// <returns>
        /// The first or default value of the query.
        /// </returns>
        private string GetOriginalGeokeyOwner(string geokey)
        {
            string sql = $@"
                            SELECT Filial_Nr FROM dbo.Werbegebietsplanung_Geometrien
                            WHERE Geokey = '{geokey}'
	                            AND Geplant_von_System = 1
                            ";

            return DbConnection.QueryFirstOrDefault<string>(sql);
        }

        /// <summary>
        /// It removes a geokey from the database
        /// </summary>
        /// <param name="planningNumber">int</param>
        /// <param name="geokey">"GK_1"</param>
        /// <param name="branchNumber">"00"</param>
        private void RemoveGeokey(int planningNumber, string geokey, string branchNumber)
        {
            string sql = $@"
                            DECLARE @Planungs_Id AS INT

                            SELECT @Planungs_Id = Planungs_ID
                            FROM dbo.Werbegebietsplanung
                            WHERE Filial_Nr = '{branchNumber}'
                                AND Planungs_Nr = {planningNumber}

                            UPDATE dbo.Werbegebietsplanung_Geometrien
                            SET Aktiv = 0,
                                Geändert_von = '{Environment.UserName}'
                            WHERE Planungs_Nr = {planningNumber}
	                            AND Filial_Nr = '{branchNumber}'
	                            AND Geokey = '{geokey}'


                            ";

            DbConnection.ExecuteScalar(sql);
        }

        /// <summary>
        /// It resets the state of a planning object to its original state
        /// </summary>
        /// <param name="planningNumber">The planning number of the planning that is currently being
        /// edited.</param>
        /// <param name="geokey">a string that is a unique identifier for a polygon</param>
        private void ResetToOriginalState(int planningNumber, string geokey)
        {
            string sql = $@"
                            UPDATE dbo.Werbegebietsplanung_Geometrien
                            SET Aktiv = 1,
                                Geändert_von = '{Environment.UserName}'
                            WHERE Planungs_Nr = {planningNumber}
                                AND Geplant_von_System = 1
	                            AND Geokey = '{geokey}'
                            ";

            DbConnection.ExecuteScalar(sql);
        }

        /// <summary>
        /// It updates the planning data of a branch with the sum of the planning data of all the
        /// geometries that belong to the branch
        /// </summary>
        /// <param name="branchNumber">The branch number of the branch that is being updated.</param>
        /// <param name="planningNumber">int</param>
        private void UpdatePlanningData(string branchNumber, int planningNumber)
        {
            try
            {
                _dbTransaction = CreateTransaction();
                string sql = $@"
                            UPDATE W
                            SET
                            w.HH_Brutto = ISNULL(Daten.HH_Brutto, 0),
                            w.HH_Netto = ISNULL(Daten.HH_Netto, 0),
                            w.Auflage = ISNULL(Daten.Auflage, 0),
                            w.Zuletzt_geändert_von = '{Environment.UserName}'
                            FROM dbo.Werbegebietsplanung w
                            Left JOIN
                            (
	                            SELECT wg.Filial_Nr, 
		                            SUM(gg.HH_Brutto) AS HH_Brutto, 
		                            SUM(gg.HH_Netto) AS HH_Netto, 
		                            SUM(gg.HH_Netto) AS Auflage
	                            FROM dbo.Werbegebietsplanung_Geometrien wg
	                            LEFT JOIN dbo.Geoservices_Geometries gg
		                            ON gg.Geokey = wg.Geokey
	                            WHERE wg.Filial_Nr = '{branchNumber}' 
                                    AND wg.Aktiv = 1
	                            GROUP BY wg.Filial_Nr
                            ) Daten
	                            ON Daten.Filial_Nr = w.Filial_Nr
                            WHERE w.Filial_Nr = '{branchNumber}'
                                AND w.Planungs_Nr = {planningNumber}
                            ";
                DbConnection.ExecuteScalar(sql, transaction: _dbTransaction);
                _dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                RollBackTransaction();
                throw;
            }

        }

        public void Finalize(int tourCoverage)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            decimal percentage = (Decimal)tourCoverage / (Decimal)100;
            ExecuteReplaceInsufficientMediaStoredProcedure(percentage);
            ExecuteFinalResultStoredProcedure(percentage);
        }

        public void ExecuteReplaceInsufficientMediaStoredProcedure(decimal tourCoverage)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            var procedure = "usp_Replace_insufficient_Media_OPTIMIZED";
            var values = new
            {
                Planungs_Nr = planningNumber,
                Konzept_Nr = 0,
                Tourenabdeckungsgrenze = tourCoverage,
                Nutzer = Environment.UserName
            };
            base.DbConnection.Execute(procedure, values, null, commandTimeout: 0, commandType: CommandType.StoredProcedure);
        }

        public void ExecuteFinalResultStoredProcedure(decimal tourCoverage)
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            var procedure = "usp_Create_Final_Result";
            var values = new
            {
                Planungs_Nr = planningNumber,
                Konzept_Nr = GetMaxKonzeptNumberByPlanningNumber(planningNumber),
                Tourenabdeckungsgrenze = tourCoverage,
                Nutzer = Environment.UserName
            };
            base.DbConnection.Execute(procedure, values, null, commandTimeout: 0, commandType: CommandType.StoredProcedure);
        }

        public List<PlanningResult> GetPlanningResult()
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            int conceptNumber = GetMaxKonzeptNumberByPlanningNumber(planningNumber);
            string sql = $@"
                        SELECT Filial_Nr as {nameof(PlanningResult.Filial_Nr)},
                               PLZ as {nameof(PlanningResult.PLZ)},
                               Medium_ID as {nameof(PlanningResult.Medium_ID)},
                               Medium_Name as {nameof(PlanningResult.Medium_Name)},
                               HH_Brutto as {nameof(PlanningResult.HH_Brutto)},
                               HH_Netto as {nameof(PlanningResult.HH_Netto)},
                               Auflage_Filiale as {nameof(PlanningResult.Auflage_Filiale)},
                               Auflage_PLZ as {nameof(PlanningResult.Auflage_PLZ)},
                               PLZ_Schärfe as {nameof(PlanningResult.PLZ_Schärfe)},
                               PLZ_Schärfe_Filiale as {nameof(PlanningResult.PLZ_Schärfe_Filiale)},
                               Reichweite as {nameof(PlanningResult.Reichweite)},
                               Reichweite_Post as {nameof(PlanningResult.Reichweite_Post)},
                               Tour as {nameof(PlanningResult.Tour)},
                               Tourenauflage as {nameof(PlanningResult.Tourenauflage)},
                               Tourenabdeckung as {nameof(PlanningResult.Tourenabdeckung)},
                               Belegung_der_PLZ as {nameof(PlanningResult.Belegung_der_PLZ)},
                               Anzahl_Touren_in_PLZ as {nameof(PlanningResult.Anzahl_Touren_in_PLZ)},
                               Belegung_der_Tour as {nameof(PlanningResult.Belegung_der_Tour)},
                               Datenquelle as {nameof(PlanningResult.Datenquelle)},
                               Ebene as {nameof(PlanningResult.Ebene)},
                               Info as {nameof(PlanningResult.Info)},
                               Status as {nameof(PlanningResult.Status)}
                        FROM dbo.Werbegebietsplanung_Resultset
                        WHERE Planungs_Nr = {planningNumber}
                            and Konzept_Nr = {conceptNumber}
                        ";

            var result = DbConnection.Query<PlanningResult>(sql).ToList();
            if (result.Any())
            {
                return result;
            }
            return new List<PlanningResult>();
        }

        public List<PlanningResultDetails> GetPlanningResultDetails()
        {
            int planningNumber = _formerPlanningNumberSet ? _formerPlanningNumber : GetMaxPlanningNumber();
            int conceptNumber = GetMaxKonzeptNumberByPlanningNumber(planningNumber);
            string sql = $@"
                        SELECT PLZ as {nameof(PlanningResultDetails.PLZ)},
                               mPLZ as {nameof(PlanningResultDetails.mPLZ)},
                               PLZ_NAME as {nameof(PlanningResultDetails.PLZ_NAME)},
                               Ortsteil as {nameof(PlanningResultDetails.Ortsteil)},
                               Medium_ID as {nameof(PlanningResultDetails.Medium_ID)},
                               Tour as {nameof(PlanningResultDetails.Tour)}
                        FROM dbo.Werbegebietsplanung_Resultset_Details
                        WHERE Planungs_Nr = {planningNumber}
                            and Konzept_Nr = {conceptNumber}
                        ";

            var result = DbConnection.Query<PlanningResultDetails>(sql).ToList();
            if (result.Any())
            {
                return result;
            }
            return new List<PlanningResultDetails>();
        }
        private IDbTransaction CreateTransaction()
        {
            try
            {
                if (DbConnection.State == ConnectionState.Closed)
                    DbConnection.Open();

                return DbConnection.BeginTransaction();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void RollBackTransaction()
        {
            if (_dbTransaction is not null)
                _dbTransaction.Rollback();
        }
    }
}
