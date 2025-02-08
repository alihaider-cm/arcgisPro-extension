using ArcGisPlannerToolbox.Core.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts
{
    public interface IGeometryRepositoryGebietsassistent : IRepository<Geometry>
    {
        void ExecuteStoredProcedureDistributionSelectionTable(int mediumId, string issue, int provideNeighborZipCodes = 0);
        List<OccupancyUnit> GetOccupancyUnits(string zipCode);
        Geometry GetSinglePolygon(string issueName, int mediumId, string mediaName, string dataSource);
        Geometry GetSinglePolygon(int mediumId, string mediaName, string dataSource);
        List<Tour> GetToursList(string id, string dataSource, bool onlyNotDigitized);
        List<Tour> GetAreasList(string occupancyUnitId);
        void SaveDigitizedOccupancyUnits(string occupancyUnitId, string userName);
        void InsertOccupancyUnitsDetail(List<OccupancyUnit> occupancyUnits, string occupancyUnitId);
        List<Geometry> GetMultiplePolygons(Media media, List<string> issues, string mediaName, string occupancyUnit);
        List<Geometry> GetMultiplePolygons(Media media, string mediaName, string occupancyUnit);
        Task<List<Geometry>> GetMultiplePolygonsAsync(Media media, string mediaName, string occupancyUnit);
        List<OccupancyUnit> GetDigitizedOccupancyUnitsList(int occupancyUnitId);
        List<OccupancyUnit> GetMissingOccupancyUnits();
        List<Tour> GetNearToursOfMissingUnits(List<string> microZipCodes);
        void DeleteOccupancyUnitsDetail(string occupancyUnitId);
        List<Tour> GetUpdatedAndChangedTours();
        Task<List<Tour>> GetUpdatedAndChangedToursAsync();
        void InsertSubTours(int? occupancyUnitId, string tourName, List<Tour> subTours);
        void InsertSubTours(int? occupancyUnitId, string tourName, List<Tour> subTours, SqlTransaction transaction);
        void InsertAreasWithNewTour(int? occupancyUnitId, string tourName, List<Tour> subTours, SqlTransaction transaction);
        void ExecuteStoredProcedureDistributionGeography(int mediumId, string mediaName, int removeArtefacts = 0);
        Geometry GetPolygonById(string id);
        DateTime GetLatestVersionOfOccupancyUnits(List<int> occupancyUnitIds);
        void ExecuteCreateAdvertisementGeographyStoredProcedure(int advertisementAreNumber, byte useTestDatabase = 0);
        bool CheckForIntersectingOccupancyUnits(List<int> units);
        DateTime GetLatestVersionOfZipCodes();
        void SaveEvaluation(string occupancyUnitId);
        void ExecuteAutoPlanner_SingleBranch(int customerId, string branchNumber, bool mindNeigbours);
        void ExecuteAutoPlanner_SingleBranch(int customerId, string branchNumber, bool mindNeigbours, int targetNumberOfCopies);
        List<string> GetAutoPlannerData();
        Task<List<string>> GetAutoPlannerDataAsync();
        List<Geometry> GetPlanningLayerPolygons(Media media, string mediaName);
        Task<List<Geometry>> GetPlanningLayerPolygonsAsync(Media media, string mediaName);
        string GetZipCodeSharpnesState(int mediaId);
        Task<string> GetZipCodeSharpnesStateAsync(int mediaId);
        SqlTransaction BeginTransaction();
        void CommitTransaction(SqlTransaction transaction);
        void RollbackTransaction(SqlTransaction transaction);
        void InsertTourAreaMapping(string tourName, List<Tour> subTours, SqlTransaction transaction);
    }
}
