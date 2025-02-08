using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ArcGisPlannerToolbox.Persistence.Repositories
{
    /// <summary>
    /// This is the repository for interaction with advertisement area data.
    /// </summary>
    public class AdvertisementAreaRepository : Repository<AdvertisementArea>, IAdvertisementAreaRepository
    {
        public AdvertisementAreaRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.MediaCenter)
        {
        }

        /// <summary>
        /// It returns a list of AdvertisementAreaAreas objects, which are populated with data from the
        /// database
        /// </summary>
        /// <param name="advertisementAreaNumber">The number of the advertisement area</param>
        /// <returns>
        /// A list of AdvertisementAreaAreas objects.
        /// </returns>
        public List<AdvertisementAreaAreas> GetAdvertisementAreaAreasByOccupancyUnitLevel(int advertisementAreaNumber)
        {
            string query = $@"
                            SELECT Werbegebiets_Nr AS {nameof(AdvertisementAreaAreas.Werbegebiets_Nr)},
                                   GKZWOQ AS {nameof(AdvertisementAreaAreas.MicroZipCode)},
                                   Teilbelegt AS {nameof(AdvertisementAreaAreas.Teilbelegt)},
                                   [Medium/Verteiler_Nr] AS {nameof(AdvertisementAreaAreas.Medium_Verteiler_Nr)},
                                   Medien_ID AS {nameof(AdvertisementAreaAreas.Medium_ID)},
                                   BBE_ID AS {nameof(AdvertisementAreaAreas.BBE_ID)},
                                   Teilauflage AS {nameof(AdvertisementAreaAreas.Teilauflage)},
                                   Gesamtauflage AS {nameof(AdvertisementAreaAreas.Gesamtauflage)},
                                   Info AS {nameof(AdvertisementAreaAreas.Info)} 
                            FROM dbo.Werbegebietsverwaltung_Werbegebiete
                            WHERE Werbegebiets_Nr = {advertisementAreaNumber}
                            ";

            var result = DbConnection.Query<AdvertisementAreaAreas>(query).ToList();

            if (result?.Count > 0)
            {
                return result;
            }
            return new List<AdvertisementAreaAreas>();
        }

        /// <summary>
        /// It returns a list of AdvertisementAreaAreas objects, which are filled with data from the
        /// database
        /// </summary>
        /// <param name="advertisementAreaNumber">The number of the advertisement area</param>
        /// <returns>
        /// A list of AdvertisementAreaAreas objects.
        /// </returns>
        public List<AdvertisementAreaAreas> GetAdvertisementAreaAreasByZipCodeLevel(int advertisementAreaNumber)
        {
            string query = $@"
                            SELECT Werbegebiets_Nr AS {nameof(AdvertisementAreaAreas.Werbegebiets_Nr)},
                                   PLZ AS {nameof(AdvertisementAreaAreas.PLZ)},
                                   Teilbelegt AS {nameof(AdvertisementAreaAreas.Teilbelegt)},
                                   [Medium/Verteiler_Nr] AS {nameof(AdvertisementAreaAreas.Medium_Verteiler_Nr)},
                                   Medien_ID AS {nameof(AdvertisementAreaAreas.Medium_ID)},
                                   Teilauflage AS {nameof(AdvertisementAreaAreas.Teilauflage)},
                                   Gesamtauflage AS {nameof(AdvertisementAreaAreas.Gesamtauflage)},
                                   Info AS {nameof(AdvertisementAreaAreas.Info)} 
                            FROM dbo.Werbegebietsverwaltung_Werbegebiete_PLZ
                            WHERE Werbegebiets_Nr = {advertisementAreaNumber}
                            ";

            var result = DbConnection.Query<AdvertisementAreaAreas>(query).ToList();

            if (result?.Count > 0)
            {
                return result;
            }
            return new List<AdvertisementAreaAreas>();
        }

        /// <summary>
        /// It returns the next available advertisement area id
        /// </summary>
        /// <returns>
        /// The next available advertisement area id.
        /// </returns>
        private string GetNewAdvertisementAreaId()
        {
            string query = $@"
                            SELECT MAX(Werbegebiets_Nr) + 1--AS Nächste_Werbegebiets_Nr
                            FROM dbo.Werbegebietsverwaltung
                            WHERE Werbegebiets_Nr < 1000000";

            return DbConnection.ExecuteScalar<string>(query);
        }

        /// <summary>
        /// It checks if a row exists in a table with the given name and branch number
        /// </summary>
        /// <param name="name">The name of the advertisement area</param>
        /// <param name="branchNumber">int</param>
        /// <returns>
        /// The result is an integer.
        /// </returns>
        public bool CheckIfAdvertisementAreaDataExists(string name, int branchNumber)
        {
            string query = $@"
                            SELECT 1
                            FROM dbo.Werbegebietsverwaltung
                            WHERE Gebietsbezeichnung = '{name}'
	                            AND Filial_ID_Superoffice = {branchNumber}
                            ";
            var result = DbConnection.ExecuteScalar<int>(query);

            if (result == 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// It returns the old advertisement area number of a given advertisement area name and branch
        /// number
        /// </summary>
        /// <param name="name">The name of the advertisement area</param>
        /// <param name="branchNumber">int</param>
        /// <returns>
        /// The old advertisement area number.
        /// </returns>
        public string GetOldAdvertisementAreaNumber(string name, int branchNumber)
        {
            string query = $@"
                            SELECT Werbegebiets_Nr
                            FROM dbo.Werbegebietsverwaltung
                            WHERE Gebietsbezeichnung = '{name}'
	                            AND Filial_ID_Superoffice = {branchNumber}
                            ";

            return DbConnection.ExecuteScalar<string>(query);
        }

        /// <summary>
        /// It saves the data of an advertisement area to the database
        /// </summary>
        /// <param name="areaNumber">int</param>
        /// <param name="AdvertisementArea">This is a class that contains all the data that needs to be
        /// inserted into the database.</param>
        /// <param name="areaType">"A" or "B"</param>
        /// <param name="stateId">1</param>
        private void SaveAdvertisementAreaAdministrationData(string areaNumber, AdvertisementArea advertisementArea, string areaType, int stateId)
        {
            string sql = $@"
                        DECLARE @status_id AS int
                        SELECT @status_id = Werbegebietsstatus_ID
                              FROM dbo.Werbegebietsstatus
                              WHERE Statusname = 'Ist'

                        INSERT INTO dbo.Werbegebietsverwaltung
                          (
                              Werbegebiets_Nr,
                              Filial_ID_Superoffice,
                              Filial_ID,
                              Werbegroßraum_ID,
                              Werbegebietstyp,
                              Werbegebietsstatus_ID,
                              Gebietsbezeichnung,
                              Aktionstyp,
                              Konzeptbezeichnung,
                              gültig_ab_Jahr_KW,
                              gültig_bis_Jahr_KW,
                              gültig_nur_in_ausgewählten_KW,
                              ausgewählte_KW,
                              Letzte_Änderung_von,
                              Letzte_Änderung_am,
                              Werbegebietsdateipfad,
                              Werbegebietsdateiname,
                              Basisgeometrie,
                              Stand,
                              ISIS_sichtbar,
                              GBConsiteTitel,
                              GBConsiteURL,
                              GBConsiteURL_erstellt_am,
                              [Aktuelles Datei-Datum],
                              [Aktueller CRC32],
                              [Aktuelle Dateigröße],
                              Letzte_Verarbeitung_am,
                              [Letzte_Verarbeitung_Datei-Datum],
                              Letzte_Verarbeitung_CRC32,
                              Letzte_Verarbeitung_Dateigröße,
                              Gebietsimport_für_KW
                          )
                          VALUES
                          ({areaNumber}, --Werbegebiets_Nr - int
                              {advertisementArea.Filial_ID_Superoffice}, --Filial_ID_Superoffice - int
                              0, --Filial_ID - int
                              0, --Werbegroßraum_ID - int
                              '{areaType}', --Werbegebietstyp - varchar(1)
                              {stateId}, --Werbegebietsstatus_ID - tinyint
                              '{advertisementArea.Gebietsbezeichnung}', --Gebietsbezeichnung - varchar(100)
                              '{advertisementArea.Aktionstyp}', --Aktionstyp - varchar(100)
                              '{advertisementArea.Konzeptbezeichnung}', --Konzeptbezeichnung - varchar(100)
                              '{advertisementArea.gültig_ab_Jahr_KW}', --gültig_ab_Jahr_KW - varchar(8)
                              '{advertisementArea.gültig_bis_Jahr_KW}', --gültig_bis_Jahr_KW - varchar(8)
                              {advertisementArea.gültig_nur_in_ausgewählten_KW}, --gültig_nur_in_ausgewählten_KW - bit
                              '{advertisementArea.ausgewählte_KW}', --ausgewählte_KW - varchar(1500)
                              '{advertisementArea.Letzte_Änderung_von}', --Letzte_Änderung_von - varchar(30)
                              '{advertisementArea.Letzte_Änderung_am}', --Letzte_Änderung_am - smalldatetime
                              '{advertisementArea.Werbegebietsdateipfad}', --Werbegebietsdateipfad - varchar(255)
                              '{advertisementArea.Werbegebietsdateiname}', --Werbegebietsdateiname - varchar(255)
                              '{advertisementArea.Basisgeometrie}', --Basisgeometrie - varchar(10)
                              '{advertisementArea.Stand}', --Stand - smalldatetime
                              {advertisementArea.ISIS_sichtbar}, --ISIS_sichtbar - bit
                              '{advertisementArea.GBConsiteTitel}', --GBConsiteTitel - varchar(255)
                              '{advertisementArea.GBConsiteURL}', --GBConsiteURL - varchar(255)
                              '{advertisementArea.GBConsiteURL_erstellt_am}', --GBConsiteURL_erstellt_am - datetime
                              '{advertisementArea.Aktuelles_Datei_Datum}', --Aktuelles Datei - Datum - datetime
                              {advertisementArea.Aktueller_CRC32}, --Aktueller CRC32 - int
                              {advertisementArea.Aktuelle_Dateigröße}, --Aktuelle Dateigröße - int
                              '{advertisementArea.Letzte_Verarbeitung_am}', --Letzte_Verarbeitung_am - datetime
                              '{advertisementArea.Letzte_Verarbeitung_Datei_Datum}', --Letzte_Verarbeitung_Datei - Datum - datetime
                              {advertisementArea.Letzte_Verarbeitung_CRC32}, --Letzte_Verarbeitung_CRC32 - int
                              {advertisementArea.Letzte_Verarbeitung_Dateigröße}, --Letzte_Verarbeitung_Dateigröße - int
                              '{advertisementArea.Gebietsimport_für_KW}'-- Gebietsimport_für_KW - varchar(1500)
                              )";

            DbConnection.Execute(sql);
        }

        /// <summary>
        /// It executes a stored procedure in the database
        /// </summary>
        private void ExecuteStoredProzedureUpdateAdvertisementAreaStatistics()
        {
            var procedure = $"dbo.sp_Werbgebietsverwaltung_Statistik_aktualisieren";
            var values = new { Full_Update = 0 };
            base.DbConnection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// It returns the ID of the advertisement state with the given name.
        /// </summary>
        /// <param name="state">string</param>
        /// <returns>
        /// The ID of the state.
        /// </returns>
        private int GetAdvertismentStateId(string state)
        {
            string sql = $@"
                        SELECT Werbegebietsstatus_ID FROM dbo.Werbegebietsstatus
	                    WHERE Statusname = '{state}'";

            return DbConnection.ExecuteScalar<int>(sql);
        }

        /// <summary>
        /// It takes a list of IDs and inserts them into a table
        /// </summary>
        /// <param name="newAreaNumber">The new area number that will be used for the new area.</param>
        /// <param name="occupancyUnitIds">List of ints</param>
        private void SaveOccupancyUnits(string newAreaNumber, List<int> occupancyUnitIds)
        {
            string sql = $@"
                            INSERT INTO dbo.Werbegebietsverwaltung_Werbegebiete
                            (
                            Werbegebiets_Nr,
                            GKZWOQ,
                            Teilbelegt,
                            [Medium/Verteiler_Nr],
                            Medien_ID,
                            BBE_ID,
                            Teilauflage,
                            Gesamtauflage,
                            Info
                            )

                            Select {newAreaNumber} AS nächste_Werbegebiewts_Nr
                            ,DBBED.mPLZ AS GKZWOQ_oder_mPLZ
                            ,0 AS Teilbelegt
                            ,0 AS MediumVerteiler_Nr
                            ,DBBE.Medium_ID
                            ,DBBE.BBE_ID
                            ,DBBE.Auflage AS Teilauflage
                            ,Gesamtauflagen.Gesamtauflage AS Gesamtauflage
                            ,NULL AS Info
                            FROM GEBIETSASSISTENT.dbo.Digitalisierte_BBE DBBE
                            INNER JOIN GEBIETSASSISTENT.dbo.Digitalisierte_BBE_Details DBBED
                            ON DBBE.BBE_ID = DBBED.BBE_ID
                            INNER JOIN(

                            SELECT Medium_ID, SUM(Auflage) AS Gesamtauflage
                            FROM GEBIETSASSISTENT.dbo.Digitalisierte_BBE
                            WHERE BBE_ID IN
                            (
                            --Liste ausgewählter BBE
                            {string.Join(", ", occupancyUnitIds)}
                            )
                            GROUP BY Medium_ID
                            ) Gesamtauflagen
                            ON  Gesamtauflagen.Medium_ID = DBBE.Medium_ID

                            WHERE DBBE.BBE_ID IN
                            (
                            --Liste ausgewählter BBE
                            {string.Join(", ", occupancyUnitIds)}
                            )";

            DbConnection.Execute(sql);
        }

        /// <summary>
        /// It takes a list of zip codes and a dictionary of zip codes and media IDs and inserts them
        /// into a table
        /// </summary>
        /// <param name="newAreaNumber">The new area number that will be used for the new area.</param>
        /// <param name="zipCodeUnits">Dictionary<string, int></param>
        private void SaveZipCodeUnits(string newAreaNumber, Dictionary<string, int> zipCodeUnits)
        {
            List<int> mediaIds = zipCodeUnits.Values.Distinct().ToList();
            foreach (var id in mediaIds)
            {
                List<string> zipCodes = zipCodeUnits.Where(z => z.Value == id).Select(z => z.Key).ToList();

                string sql = $@"
                            INSERT INTO mediacenter.dbo.Werbegebietsverwaltung_Werbegebiete_PLZ
                            (
                                Werbegebiets_Nr,
                                GKZ,
                                PLZ,
                                Teilbelegt,
                                [Medium/Verteiler_Nr],
                                Medien_ID,
                                Teilauflage,
                                Gesamtauflage,
                                Info
                            )
                            SELECT {newAreaNumber} AS nächste_Werbegebiewts_Nr, -- Werbegebiets_Nr - int
                                   RIGHT(g.Tour,5) AS GKZWOQ_oder_PLZ,    -- GKZ - varchar(8)
                                   RIGHT(g.Tour,5) AS PLZ,                       -- PLZ - varchar(14)
                                   0 AS Teilbelegt,               -- Teilbelegt - bit
                                   0 AS MediumVerteiler_Nr,       -- Medium/Verteiler_Nr - smallint
                                   g.Medium_ID,                           -- Medien_ID - int
                                   g.Auflage,                     -- Teilauflage - int
                                   t.Gesamtauflage,               -- Gesamtauflage - int
                                   NULL As Info                   -- Info - varchar(255)
                            FROM GEBIETSASSISTENT.dbo.Verbreitungsgebiete_mit_Geometrie g
                                INNER JOIN
                                (
                                    SELECT SUM(Auflage) AS Gesamtauflage,
                                           Medium_ID
                                    FROM GEBIETSASSISTENT.dbo.Verbreitungsgebiete_mit_Geometrie
                                    WHERE RIGHT(Tour, 5) IN ('{string.Join("', '", zipCodes)}')
                                    GROUP BY Medium_ID
                                ) AS t
                                    ON t.Medium_ID = g.Medium_ID
                            WHERE g.Medium_ID = {id}
                                  AND Belegungseinheit = 'PLZ'
                                  AND RIGHT(g.Tour,5) IN ('{string.Join("', '", zipCodes)}')";

                DbConnection.Execute(sql);
            }
        }

        /// <summary>
        /// It saves the advertisement area data to the database
        /// </summary>
        /// <param name="AdvertisementArea">This is the main area that is being saved.</param>
        /// <param name="areaUnits">List of ints</param>
        /// <param name="neighborBranchAreas">A list of AdvertisementArea objects.</param>
        /// <param name="updateData">if true, the area will be updated, if false, a new area will be
        /// created</param>
        public void SaveAdvertisementArea(AdvertisementArea advertisementArea, List<int> areaUnits, List<AdvertisementArea> neighborBranchAreas = null, bool updateData = false)
        {
            if (areaUnits.Count > 0)
            {
                string areaType = "E";
                if (neighborBranchAreas.Count > 0)
                {
                    areaType = "G";
                }

                string newAreaNumber;
                if (updateData)
                {
                    string oldAreaNumber = GetOldAdvertisementAreaNumber(advertisementArea.Gebietsbezeichnung,
                    advertisementArea.Filial_ID_Superoffice);
                    newAreaNumber = oldAreaNumber;
                    DeleteAdvertisementAreaData(newAreaNumber);
                }
                else
                {
                    newAreaNumber = GetNewAdvertisementAreaId();
                }

                int stateId = GetAdvertismentStateId(advertisementArea.Werbegebietsstatus);

                SaveAdvertisementAreaAdministrationData(newAreaNumber, advertisementArea, areaType, stateId);

                SaveOccupancyUnits(newAreaNumber, areaUnits);

                if (neighborBranchAreas?.Count > 0)
                {
                    foreach (var area in neighborBranchAreas)
                    {
                        SaveAdvertisementAreaAdministrationData(newAreaNumber, area, areaType, stateId);
                    }
                }
                ExecuteStoredProzedureUpdateAdvertisementAreaStatistics();
            }
        }

        /// <summary>
        /// It saves advertisement areas to the database
        /// </summary>
        /// <param name="AdvertisementArea">This is the main area that is being saved.</param>
        /// <param name="zipCodeUnits">Dictionary<string, int></param>
        /// <param name="neighborBranchAreas">A list of AdvertisementArea objects.</param>
        /// <param name="updateData">if true, the area will be updated, if false, a new area will be
        /// created</param>
        public void SaveAdvertisementArea(AdvertisementArea advertisementArea, Dictionary<string, int> zipCodeUnits, List<AdvertisementArea> neighborBranchAreas = null, bool updateData = false)
        {
            if (zipCodeUnits.Count > 0)
            {
                string areaType = "E";
                if (neighborBranchAreas.Count > 0)
                {
                    areaType = "G";
                }

                string newAreaNumber;
                if (updateData)
                {
                    string oldAreaNumber = GetOldAdvertisementAreaNumber(advertisementArea.Gebietsbezeichnung,
                    advertisementArea.Filial_ID_Superoffice);
                    newAreaNumber = oldAreaNumber;
                    DeleteAdvertisementAreaData(newAreaNumber);
                }
                else
                {
                    newAreaNumber = GetNewAdvertisementAreaId();
                }

                int stateId = GetAdvertismentStateId(advertisementArea.Werbegebietsstatus);

                SaveAdvertisementAreaAdministrationData(newAreaNumber, advertisementArea, areaType, stateId);

                SaveZipCodeUnits(newAreaNumber, zipCodeUnits);

                if (neighborBranchAreas?.Count > 0)
                {
                    foreach (var area in neighborBranchAreas)
                    {
                        SaveAdvertisementAreaAdministrationData(newAreaNumber, area, areaType, stateId);
                    }
                }
                ExecuteStoredProzedureUpdateAdvertisementAreaStatistics();
            }
        }

        /// <summary>
        /// It saves advertisement areas
        /// </summary>
        /// <param name="AdvertisementArea">This is the main area that is being saved.</param>
        /// <param name="zipCodeUnits">Dictionary<string, int></param>
        /// <param name="areaUnits">List of ints</param>
        /// <param name="neighborBranchAreas">A list of AdvertisementArea objects.</param>
        /// <param name="updateData">if true, the area will be updated, if false, a new area will be
        /// created</param>
        public void SaveAdvertisementArea(AdvertisementArea advertisementArea, Dictionary<string, int> zipCodeUnits, List<int> areaUnits, List<AdvertisementArea> neighborBranchAreas = null, bool updateData = false)
        {
            if (zipCodeUnits.Count > 0)
            {
                string areaType = "E";
                if (neighborBranchAreas.Count > 0)
                {
                    areaType = "G";
                }

                string newAreaNumber;
                if (updateData)
                {
                    string oldAreaNumber = GetOldAdvertisementAreaNumber(advertisementArea.Gebietsbezeichnung,
                    advertisementArea.Filial_ID_Superoffice);
                    newAreaNumber = oldAreaNumber;
                    DeleteAdvertisementAreaData(newAreaNumber);
                }
                else
                {
                    newAreaNumber = GetNewAdvertisementAreaId();
                }

                int stateId = GetAdvertismentStateId(advertisementArea.Werbegebietsstatus);

                SaveAdvertisementAreaAdministrationData(newAreaNumber, advertisementArea, areaType, stateId);

                SaveZipCodeUnits(newAreaNumber, zipCodeUnits);
                SaveOccupancyUnits(newAreaNumber, areaUnits);

                if (neighborBranchAreas?.Count > 0)
                {
                    foreach (var area in neighborBranchAreas)
                    {
                        SaveAdvertisementAreaAdministrationData(newAreaNumber, area, areaType, stateId);
                    }
                }
                ExecuteStoredProzedureUpdateAdvertisementAreaStatistics();
            }
        }

        /// <summary>
        /// It deletes the data from the database
        /// </summary>
        /// <param name="areaNumber">The number of the area that is to be deleted</param>
        private void DeleteAdvertisementAreaData(string areaNumber)
        {
            string sql = $@"
                        DELETE FROM dbo.Werbegebietsverwaltung
                        WHERE Werbegebiets_Nr = {areaNumber}
                        ";

            DbConnection.Execute(sql);
        }
    }
}
