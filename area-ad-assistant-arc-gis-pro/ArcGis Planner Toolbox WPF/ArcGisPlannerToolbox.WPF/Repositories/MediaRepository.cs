using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.Persistence.Repositories
{
    /// <summary>
    /// This is the repository for interaction with media data.
    /// </summary>
    public class MediaRepository : Repository<Media>, IMediaRepository
    {
        public MediaRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.MediaCenter)
        {
        }

        //Example code can be found --> https://github.com/ericdc1/Dapper.SimpleCRUD/
        /// <summary>
        /// I'm trying to get all the media from the database and I'm trying to get the appearance rhythm
        /// of each media
        /// </summary>
        /// <returns>
        /// A list of Media objects.
        /// </returns>
        public List<Media> GetAllMedia()
        {
            string query = $@"SELECT 
	                            Result.*,
	                            MR.BEZEICHNUNG AS {nameof(AppearanceRhythm.Name)},
	                            MR.RH_ID AS {nameof(AppearanceRhythm.Id)}
                            FROM
	                            (SELECT DISTINCT
                                    M.MEDIUM_ID AS {nameof(Media.Id)}, 
                                    M.MEDIUM_NAME AS {nameof(Media.Name)}, 
                                    M.MEDIUM_TYP AS {nameof(Media.MediaType)}, 
                                    M.STREUART AS {nameof(Media.MediaSpreadingType)}, 
                                    M.AKTIV AS {nameof(Media.IsActive)}, 
                                    M.MEDIUM_ERSCHEINWEISE,
                                    M.MEDIUM_ERSCHEINT_WOCHENANFANG AS {nameof(Media.AppersBeginningOfWeek)}, 
                                    M.MEDIUM_ERSCHEINT_WOCHENMITTE AS {nameof(Media.AppersMiddleOfWeek)}, 
                                    M.MEDIUM_ERSCHEINT_WOCHENENDE AS {nameof(Media.AppersEndOfWeek)}, 
                                    M.Verbreitungsgebiet_Datenquelle AS {nameof(Media.DistributionAreaSource)},
                                    M.Verbreitungsgebiet_Letzte_Änderung_am AS {nameof(Media.Status)},
                                    M.KLEINSTE_BELEGUNGSEINHEIT AS {nameof(Media.SmallestOccupancyUnit)},
                                    CASE WHEN VBG.VERBREITUNGSGEBIET_MEDIUM_ID IS NULL THEN 0
	                                    ELSE 1 END AS {nameof(Media.HasDistributionArea)}
                                FROM
                                    dbo.MEDIEN AS M
                                LEFT JOIN dbo.VERBREITUNGSGEBIETE AS VBG ON M.MEDIUM_ID = VBG.VERBREITUNGSGEBIET_MEDIUM_ID
                                Where M.Eingestellt = 0) AS Result
                            LEFT JOIN dbo.MEDIEN_ERSCHEINRHYTHMUS AS MR ON Result.MEDIUM_ERSCHEINWEISE = MR.RH_ID
                            ";

            var result = base.DbConnection.Query<Media, AppearanceRhythm, Media>(query,
                (Result, MR) => { Result.AppearanceRhythm = MR; return Result; }
                , splitOn: $"{nameof(AppearanceRhythm.Name)}");

            return result.ToList();
        }
        public async Task<List<Media>> GetAllMediaAsync()
        {
            string query = $@"SELECT 
                        Result.*,
                        MR.BEZEICHNUNG AS {nameof(AppearanceRhythm.Name)},
                        MR.RH_ID AS {nameof(AppearanceRhythm.Id)}
                    FROM
                        (SELECT DISTINCT
                            M.MEDIUM_ID AS {nameof(Media.Id)}, 
                            M.MEDIUM_NAME AS {nameof(Media.Name)}, 
                            M.MEDIUM_TYP AS {nameof(Media.MediaType)}, 
                            M.STREUART AS {nameof(Media.MediaSpreadingType)}, 
                            M.AKTIV AS {nameof(Media.IsActive)}, 
                            M.MEDIUM_ERSCHEINWEISE,
                            M.MEDIUM_ERSCHEINT_WOCHENANFANG AS {nameof(Media.AppersBeginningOfWeek)}, 
                            M.MEDIUM_ERSCHEINT_WOCHENMITTE AS {nameof(Media.AppersMiddleOfWeek)}, 
                            M.MEDIUM_ERSCHEINT_WOCHENENDE AS {nameof(Media.AppersEndOfWeek)}, 
                            M.Verbreitungsgebiet_Datenquelle AS {nameof(Media.DistributionAreaSource)},
                            M.Verbreitungsgebiet_Letzte_Änderung_am AS {nameof(Media.Status)},
                            M.KLEINSTE_BELEGUNGSEINHEIT AS {nameof(Media.SmallestOccupancyUnit)},
                            CASE WHEN VBG.VERBREITUNGSGEBIET_MEDIUM_ID IS NULL THEN 0
                                ELSE 1 END AS {nameof(Media.HasDistributionArea)}
                        FROM
                            dbo.MEDIEN AS M
                        LEFT JOIN dbo.VERBREITUNGSGEBIETE AS VBG ON M.MEDIUM_ID = VBG.VERBREITUNGSGEBIET_MEDIUM_ID
                        Where M.Eingestellt = 0) AS Result
                    LEFT JOIN dbo.MEDIEN_ERSCHEINRHYTHMUS AS MR ON Result.MEDIUM_ERSCHEINWEISE = MR.RH_ID";

            var result = await base.DbConnection.QueryAsync<Media, AppearanceRhythm, Media>(query,
                (Result, MR) => { 
                    Result.AppearanceRhythm = MR; 
                    System.Diagnostics.Debug.WriteLine($"Mapped DistributionAreaSource: {Result.DistributionAreaSource}");
                    return Result; 
                }
                , splitOn: $"{nameof(AppearanceRhythm.Name)}");

            return result.ToList();
        }

        /// <summary>
        /// It takes a string as an argument and returns a list of media objects
        /// </summary>
        /// <param name="namePattern">"The"</param>
        /// <returns>
        /// A list of Media objects.
        /// </returns>
        public List<Media> FindByNamePattern(string namePattern)
        {
            string expression = "%" + EncodeLike(namePattern) + "%";
            return Find("where MEDIUM_NAME like @Name and Eingestellt = 0", new { Name = expression }).ToList();
        }

        /// <summary>
        /// Finds all media that are not yet in the database and have the same ID as the given ID
        /// </summary>
        /// <param name="idPattern">The ID of the media to be found</param>
        /// <returns>
        /// A list of Media objects.
        /// </returns>
        public List<Media> FindContinuedByID(int idPattern)
        {
            return Find("where MEDIUM_ID = @MediaId and Eingestellt = 0", new { MediaId = idPattern }).ToList();
        }

        /// <summary>
        /// It returns a list of Media objects that match the idPattern
        /// </summary>
        /// <param name="idPattern">The ID of the media you want to find.</param>
        /// <returns>
        /// A list of Media objects.
        /// </returns>
        public List<Media> FindAllByID(int idPattern)
        {
            return Find("where MEDIUM_ID = @MediaId", new { MediaId = idPattern }).ToList();
        }

        /// <summary>
        /// Get all media, where the id is equal to the id passed in, and return the first one. If the
        /// media is not null, set the area to the list of areas
        /// </summary>
        /// <param name="id">The id of the media</param>
        /// <returns>
        /// A list of Media objects with the DistributionArea property filled with a list of
        /// DistributionArea objects.
        /// </returns>
        public Media GetWithDistributionArea(int id)
        {
            var media = GetAllMedia().Where(m => m.Id == id).FirstOrDefault();
            var area = DbConnection.GetList<DistributionArea>("where VERBREITUNGSGEBIET_MEDIUM_ID = @MediaId", new { MediaId = id });
            if (media != null)
                media.Area = area.ToList();

            return media;
        }

        public async Task<Media> GetWithDistributionAreaAsync(int id)
        {
            var media = (await GetAllMediaAsync()).Where(m => m.Id == id).FirstOrDefault();
            var area = await DbConnection.GetListAsync<DistributionArea>("where VERBREITUNGSGEBIET_MEDIUM_ID = @MediaId", new { MediaId = id });
            if (media != null)
                media.Area = area.ToList();

            return media;
        }

        /// <summary>
        /// Get the media with the given id and the distribution area for the given data source
        /// </summary>
        /// <param name="id">the id of the media</param>
        /// <param name="dataSource">"S"</param>
        /// <returns>
        /// A list of DistributionArea objects.
        /// </returns>
        public Media GetWithDistributionArea(int id, string dataSource)
        {
            var media = Get(id);
            var area = DbConnection.GetList<DistributionArea>("where VERBREITUNGSGEBIET_MEDIUM_ID = @MediaId", new { MediaId = id });
            media.Area = area.Where(d => d.DataSource == dataSource).ToList();

            return media;
        }

        /// <summary>
        /// It takes a location (e.g. "Berlin") and returns a list of media IDs that are available in
        /// that location
        /// </summary>
        /// <param name="location">"Berlin"</param>
        /// <returns>
        /// A list of integers.
        /// </returns>
        public async Task<List<int>> FindMediaIdByLocation(string location)
        {
            string expression = "%" + EncodeLike(location) + "%";
            string query = String.Format(@"SELECT VG.VERBREITUNGSGEBIET_MEDIUM_ID MID
                                        FROM dbo.VERBREITUNGSGEBIETE VG
                                        INNER JOIN  
                                        (
                                        SELECT PLZ FROM Geodaten.dbo.PLZ_LEITDATEI
                                        WHERE PLZ LIKE  '{0}'
                                        OR ORT LIKE '{1}' 
                                        GROUP BY PLZ
                                        ) Ergebnisliste_PLZ
                                            ON VG.VERBREITUNGSGEBIET_PLZ = Ergebnisliste_PLZ.PLZ COLLATE DATABASE_DEFAULT
                                        AND (VG.LKZ IS NULL OR VG.LKZ IN ('D','DE'))
                                        INNER JOIN dbo.MEDIEN M
                                            ON M.Verbreitungsgebiet_Datenquelle = VG.DATENQUELLE
                                        Where M.aktiv = 1 AND M.EINGESTELLT = 0
                                        GROUP BY  VG.VERBREITUNGSGEBIET_MEDIUM_ID", expression, expression);

            var mediaIds = await base.DbConnection.QueryAsync<int>(query);

            return mediaIds.ToList();
        }

        /// <summary>
        /// It returns a list of strings
        /// </summary>
        /// <param name="selSpreadingTypes">A list of strings that are the spreading types</param>
        /// <param name="selTimeOfAppearance">List of strings</param>
        /// <returns>
        /// A list of strings.
        /// </returns>
        public List<string> GetAvailableMediaTypes(List<string> selSpreadingTypes, List<string> selTimeOfAppearance)
        {
            string paramString = QueryStringBuilder(selSpreadingTypes, selTimeOfAppearance, "STREUART", null);

            string query = $@"SELECT * FROM
                                (SELECT DISTINCT
	                                M.MEDIUM_TYP
                                FROM dbo.MEDIEN AS M 
                                LEFT JOIN dbo.VERBREITUNGSGEBIETE AS VBG ON M.MEDIUM_ID = VBG.VERBREITUNGSGEBIET_MEDIUM_ID
                                WHERE VBG.VERBREITUNGSGEBIET_MEDIUM_ID IS NOT NULL
                                    AND M.Eingestellt = 0
                                {paramString}) AS Result
                            WHERE Result.MEDIUM_TYP IS NOT NULL
                            ORDER BY Result.MEDIUM_TYP";
            return base.DbConnection.Query<string>(query).ToList();

        }

        /// <summary>
        /// It returns a list of strings that are the available spreading types for the selected media
        /// types and time of appearance
        /// </summary>
        /// <param name="selMediaTypes">List of selected media types</param>
        /// <param name="selTimeOfAppearance">A list of strings that are the selected time of
        /// appearance.</param>
        /// <returns>
        /// A list of strings.
        /// </returns>
        public List<string> GetAvailableSpreadingTypes(List<string> selMediaTypes, List<string> selTimeOfAppearance)
        {
            string paramString = QueryStringBuilder(selMediaTypes, selTimeOfAppearance, "MEDIUM_TYP", null);

            string query = $@"SELECT * FROM
                                (SELECT DISTINCT 
                                    M.STREUART
                                FROM dbo.MEDIEN as M
                                LEFT JOIN dbo.VERBREITUNGSGEBIETE AS VBG ON M.MEDIUM_ID = VBG.VERBREITUNGSGEBIET_MEDIUM_ID
                                WHERE VBG.VERBREITUNGSGEBIET_MEDIUM_ID IS NOT NULL
                                    AND M.Eingestellt = 0
                                {paramString}) AS Result
                            WHERE Result.STREUART IS NOT NULL
                            ORDER BY Result.STREUART";

            var availableSpreadingTypes = base.DbConnection.Query<string>(query);
            return availableSpreadingTypes.ToList();
        }

        /// <summary>
        /// It returns a list of strings that represent the available times of appearance of the
        /// selected media types and spreading types
        /// </summary>
        /// <param name="selMediaTypes">List of selected media types</param>
        /// <param name="selSpreadingTypes">A list of spreading types (e.g. "Print", "Online", "TV",
        /// etc.)</param>
        /// <returns>
        /// A list of strings.
        /// </returns>
        public List<string> GetAvailableTimesOfAppearance(List<string> selMediaTypes, List<string> selSpreadingTypes)
        {
            string paramString = QueryStringBuilder(selMediaTypes, selSpreadingTypes, "MEDIUM_TYP", "STREUART");

            string query = $@"SELECT * FROM
	                        (SELECT DISTINCT
		                        CASE WHEN M.MEDIUM_ERSCHEINT_WOCHENANFANG = 1 THEN 'Wochenanfang' 
                                        WHEN M.MEDIUM_ERSCHEINT_WOCHENMITTE = 1 THEN 'Wochenmitte'
                                        WHEN M.MEDIUM_ERSCHEINT_WOCHENENDE = 1 THEN 'Wochenende'
                                        ELSE NULL END AS Appearance
                            FROM dbo.MEDIEN AS M
                            LEFT JOIN dbo.VERBREITUNGSGEBIETE AS VBG ON M.MEDIUM_ID = VBG.VERBREITUNGSGEBIET_MEDIUM_ID
                            WHERE VBG.VERBREITUNGSGEBIET_MEDIUM_ID IS NOT NULL
                                AND M.Eingestellt = 0
                            {paramString}) AS Result
                            WHERE Result.Appearance IS NOT NULL
                            ORDER BY CASE 
                                WHEN Result.Appearance = 'Wochenanfang' THEN 1
                                WHEN Result.Appearance = 'Wochenmitte' THEN 2
                                WHEN Result.Appearance = 'Wochenende' THEN 3 END";

            var availableTimesOfAppearance = base.DbConnection.Query<string>(query);
            return availableTimesOfAppearance.ToList();
        }

        public async Task<List<string>> GetAvailableTimesOfAppearanceAsync(List<string> selMediaTypes, List<string> selSpreadingTypes)
        {
            string paramString = QueryStringBuilder(selMediaTypes, selSpreadingTypes, "MEDIUM_TYP", "STREUART");

            string query = $@"SELECT * FROM
	                        (SELECT DISTINCT
		                        CASE WHEN M.MEDIUM_ERSCHEINT_WOCHENANFANG = 1 THEN 'Wochenanfang' 
                                        WHEN M.MEDIUM_ERSCHEINT_WOCHENMITTE = 1 THEN 'Wochenmitte'
                                        WHEN M.MEDIUM_ERSCHEINT_WOCHENENDE = 1 THEN 'Wochenende'
                                        ELSE NULL END AS Appearance
                            FROM dbo.MEDIEN AS M
                            LEFT JOIN dbo.VERBREITUNGSGEBIETE AS VBG ON M.MEDIUM_ID = VBG.VERBREITUNGSGEBIET_MEDIUM_ID
                            WHERE VBG.VERBREITUNGSGEBIET_MEDIUM_ID IS NOT NULL
                                AND M.Eingestellt = 0
                            {paramString}) AS Result
                            WHERE Result.Appearance IS NOT NULL
                            ORDER BY CASE 
                                WHEN Result.Appearance = 'Wochenanfang' THEN 1
                                WHEN Result.Appearance = 'Wochenmitte' THEN 2
                                WHEN Result.Appearance = 'Wochenende' THEN 3 END";

            var availableTimesOfAppearance = await base.DbConnection.QueryAsync<string>(query);
            return availableTimesOfAppearance.ToList();
        }

        /// <summary>
        /// It returns a list of strings that are the times of appearance of a medium
        /// </summary>
        /// <param name="selMediaTypes">List of selected media types</param>
        /// <param name="selSpreadingTypes">List of selected spreading types</param>
        /// <returns>
        /// A list of strings.
        /// </returns>
        public List<string> GetAvailableAppearanceRhythm(List<string> selMediaTypes, List<string> selSpreadingTypes)
        {
            string paramString = QueryStringBuilder(selMediaTypes, selSpreadingTypes, "MEDIUM_TYP", "STREUART");

            string query = $@"SELECT * FROM
                                (SELECT DISTINCT 
                                    M.MEDIUM_ERSCHEINWEISE
                                FROM dbo.MEDIEN as M
                                LEFT JOIN dbo.VERBREITUNGSGEBIETE AS VBG ON M.MEDIUM_ID = VBG.VERBREITUNGSGEBIET_MEDIUM_ID
                                WHERE VBG.VERBREITUNGSGEBIET_MEDIUM_ID IS NOT NULL
                                    AND M.Eingestellt = 0
	                            {paramString}
                                ) AS Result
                            WHERE Result.MEDIUM_ERSCHEINWEISE IS NOT NULL
                            ORDER BY Result.MEDIUM_ERSCHEINWEISE";

            var availableTimesOfAppearance = base.DbConnection.Query<string>(query);
            return availableTimesOfAppearance.ToList();
        }

        /// <summary>
        /// It takes two lists of strings and two strings and returns a string
        /// </summary>
        /// <param name="queryList1">A list of strings that contains the query parameters for the first
        /// row.</param>
        /// <param name="queryList2">List of strings that contain the query parameters</param>
        /// <param name="row1">The first row of the table</param>
        /// <param name="row2">"row2"</param>
        /// <returns>
        /// A string that is used to filter the data in the DataGridView.
        /// </returns>
        private string QueryStringBuilder(List<string> queryList1, List<string> queryList2, string row1, string row2)
        {
            string start = "";
            if (queryList1 != null || queryList2 != null)
            {
                if ((queryList1 != null && queryList1.Count > 0) || (queryList2 != null && queryList2.Count > 0))
                {
                    start += "AND";
                }

                string rowFilter1 = "";
                if (queryList1 != null && queryList1.Count > 0)
                {
                    if (!queryList1[0].Contains("ERSCHEINT"))
                    {
                        rowFilter1 += GetStandardRowParams(queryList1, row1);
                    }
                    else
                    {
                        if (row1 != null)
                        {
                            rowFilter1 += GetTimeOfAppearanceString(queryList1);
                        }
                    }
                }

                string connector = "";
                if (((queryList1 != null && queryList1.Count > 0) && (queryList2 != null && queryList2.Count > 0)))
                {
                    connector += "AND";
                }


                string rowFilter2 = "";
                if (queryList2 != null && queryList2.Count > 0)
                {
                    if (!queryList2[0].Contains("ERSCHEINT"))
                    {
                        rowFilter2 += GetStandardRowParams(queryList2, row2);
                    }
                    else
                    {
                        if (row2 != null)
                        {
                            rowFilter2 += GetTimeOfAppearanceString(queryList2);
                        }
                    }
                }
                return start + " " + rowFilter1 + " " + connector + " " + rowFilter2;
            }
            return start;
        }

        /// <summary>
        /// It takes a list of strings and a string and returns a string
        /// </summary>
        /// <param name="queryList">A list of strings that are the values of the row you want to filter
        /// by.</param>
        /// <param name="row">The name of the column in the table</param>
        /// <returns>
        /// A string that is a SQL query.
        /// </returns>
        private string GetStandardRowParams(List<string> queryList, string row)
        {
            string rowFilter1 = "";
            if (queryList != null && queryList.Count > 0)
            {
                rowFilter1 += $"M.{row} = '";
                rowFilter1 += String.Join($"' AND M.{row} = '", queryList);
                rowFilter1 += "'";
            }
            return rowFilter1;
        }

        /// <summary>
        /// It takes a list of strings and returns a string
        /// </summary>
        /// <param name="selTimeOfAppearance">List of strings</param>
        /// <returns>
        /// A string
        /// </returns>
        private string GetTimeOfAppearanceString(List<string> selTimeOfAppearance)
        {
            string selTimeOfAppearanceString = "";
            if (selTimeOfAppearance != null)
            {
                foreach (var time in selTimeOfAppearance)
                {
                    if (time == "Wochenanfang")
                    {
                        selTimeOfAppearanceString += " M.MEDIUM_ERSCHEINT_WOCHENANFANG = 1";
                    }
                    else
                    {
                        if (time == "Wochenmitte")
                        {
                            selTimeOfAppearanceString += " M.MEDIUM_ERSCHEINT_WOCHENMITTE = 1";
                        }
                        else
                        {
                            selTimeOfAppearanceString += " M.MEDIUM_ERSCHEINT_WOCHENENDE = 1";
                        }
                    }
                    if (time != selTimeOfAppearance[selTimeOfAppearance.Count - 1])
                        selTimeOfAppearanceString += " AND";
                }
            }
            return selTimeOfAppearanceString;
        }

        /// <summary>
        /// It replaces all instances of the character "[" with "[[]" and all instances of the character
        /// "%" with "[%]"
        /// </summary>
        /// <param name="expression">The string to be encoded.</param>
        /// <returns>
        /// The expression is being returned with the [ and % replaced with [[] and [%].
        /// </returns>
        private string EncodeLike(string expression)
        {
            return expression.Replace("[", "[[]")
                             .Replace("%", "[%]");
        }

        /// <summary>
        /// It returns a list of objects of type SmallestOccUnit
        /// </summary>
        /// <returns>
        /// A list of SmallestOccUnit objects.
        /// </returns>
        public List<SmallestOccUnit> GetSmallestOccUnitsList()
        {
            string query = $@"
                            SELECT Belegungseinheit_ID AS {nameof(SmallestOccUnit.Id)}
                                ,Belegungseinheit AS {nameof(SmallestOccUnit.OccupancyUnit)}
                                ,Reihenfolge AS {nameof(SmallestOccUnit.Ordered)}
                                ,aktiv AS {nameof(SmallestOccUnit.IsActive)}
                            FROM MEDIEN_BELEGUNGSEINHEITEN
                            ";

            var result = base.DbConnection.Query<SmallestOccUnit>(query).ToList();
            if (result != null)
            {
                return result;
            }
            return new List<SmallestOccUnit>();
        }

        /// <summary>
        /// It updates the smallest occupancy unit of a media in the database
        /// </summary>
        /// <param name="id">int</param>
        /// <param name="newOccUnit">string</param>
        public void UpdateMediasSmallesOccUnit(int id, string newOccUnit)
        {
            var unitString = newOccUnit == "---" ? "NULL" : newOccUnit;
            string sql = $@"
                        UPDATE dbo.MEDIEN
                        SET 
                        ,[KLEINSTE_BELEGUNGSEINHEIT] = {unitString}
                        ,[KLEINSTE_BELEGUNGSEINHEIT_AEND_AM] = GetDate()
                        ,[KLEINSTE_BELEGUNGSEINHEIT_AEND_VON] = {Environment.UserName} 
                        WHERE [MEDIUM_ID] = {id}
                        ";

            base.DbConnection.Execute(sql);
        }

        /// <summary>
        /// It returns a list of MediaDigitizationState objects, which are created from the result of a
        /// stored procedure
        /// </summary>
        /// <param name="spreadingType">This is a string that can be null.</param>
        /// <returns>
        /// A list of MediaDigitizationState objects.
        /// </returns>
        public async Task<IEnumerable<MediaDigitizationState>> GetStateOfDigitizationAsync(string spreadingType = null)
        {
            IEnumerable<MediaDigitizationState> result = new List<MediaDigitizationState>();
            DbConnection.Open();
            var procedure = "report.usp_Report_Monitoring_Digitalisierung_der_Belegungseinheiten";

            if (spreadingType != null)
            {
                var values = new
                {
                    rpGattungStreuart = spreadingType
                };
                result = await DbConnection.QueryAsync<MediaDigitizationState>(procedure, values, commandType: CommandType.StoredProcedure, commandTimeout: 0);
            }
            else
            {
                result = await DbConnection.QueryAsync<MediaDigitizationState>(procedure, commandType: CommandType.StoredProcedure, commandTimeout: 0);
            }

            return result;

        }

        /// <summary>
        /// It returns a list of MediaDigitizationState objects
        /// </summary>
        /// <param name="spreadingType">The type of media (e.g. "Buch", "Zeitschrift", "Zeitung",
        /// "Karte", "Musik", "Video", "Elektronisches Dokument", "Sonstiges")</param>
        /// <returns>
        /// A list of MediaDigitizationState objects.
        /// </returns>
        public List<MediaDigitizationState> GetStateOfDigitization(string spreadingType = null)
        {
            var procedure = "report.usp_Report_Monitoring_Digitalisierung_der_Belegungseinheiten";

            if (spreadingType != null)
            {
                var values = new
                {
                    rpGattungStreuart = spreadingType
                };
                return DbConnection.Query<MediaDigitizationState>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
            }
            else
            {
                return DbConnection.Query<MediaDigitizationState>(procedure, commandType: CommandType.StoredProcedure).ToList();
            }
        }
    }
}
