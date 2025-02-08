using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.Core.Contracts
{
    public interface IPlanningRepository : IRepository<PlanningData>
    {
        int FormerPlanningNumber { get; set; }

        void ExecuteAutoPlannerStoredProcedure(int analysisId, string planningLevel, bool allowCanibalization, int customerPercentage);
        Task ExecuteAutoPlannerStoredProcedureAsync(int analysisId, string planningLevel, bool allowCanibalization, int customerPercentage);
        void ExecuteStoredProcedurePlanningSelectionTable(string branchNumber, int analysisId, int customerId);
        Task ExecuteUploadBranchDataToDatabase(int customerId, CustomerBranch branch);
        string GetAddressCircleByBranchNumber(string branchNumber);
        List<PlanningData> GetFormerPlannings(int customerId, string planningType);
        List<PlanningData> GetPlanningData();
        List<PlanningData> GetPlanningData(int planningNumber);
        List<PlanningData> GetPlanningDataByPlanningNumberAndConceptNumber(int planningNumber, int conceptNumber);
        List<PlanningDataArea> GetPlanningDataAreasByBranchNumber(string branchNumber);
        List<PlanningDataArea> GetPlanningDataFromAllBranches();
        List<PlanningDataArea> GetOverlappingPlanningDataByBranchNumber(string branchNumber, int analysisId);
        List<PlanningDataArea> GetGeokysFromTempZipCodeTable();
        List<PlanningDataArea> GetCurrentOwnersOfGeokeys(List<string> geokeys);
        void MoveAreaToDifferentBranch(List<PlanningDataArea> currentOwners, string newBranchNumber);
        void AddUnselectedAreaToBranch(List<string> geokeys, string branchNumber);
        void RemoveSelectedAreaFromBranch(List<string> geokeys, string branchNumber);
        Task<int> GetBranchesToBePlannedCountAsync();
        Task<int> GetCurrentlyPlannedBranchesCountAsync(DateTime startTime);
        Task<int> GetFinishedBranchesCountAsync(DateTime startTime);
        int GetBranchesToBePlannedCount(int customerId);
        int GetCurrentlyPlannedBranchesCount(DateTime startTime);
        int GetFinishedBranchesCount(DateTime startTime);
        void DeleteFormerBranchSelection(int customerId);
        Task ExecuteAddressCirclePlanner(int conceptNumber, int analysisId, int minIntersections, int maxDifference);
        int GetFinishedAddressCirclesCount(DateTime startTime);
        List<AddressCircle> GetAllAddressCircles();
        List<AddressCircle> GetAddressCirclesByPlanningNumber(int planningNumber);
        List<AddressCicleDetails> GetAddressCircleDetailsByBranchNumber(string branchNumber);
        List<string> GetRelevantBranchesExcludedFromAddressCircle(List<string> branchNumbers, string addressCircle);
        List<AddressCircleArea> GetAddressCircleAreas();
        void ExecutePrepareAddressCircleAreas();
        bool CheckAddressCircleNeed(int customerId);
        void ExecuteAutoPlannerForNewBranchesStoredProcedure(int analysisId, string planningLevel, int planninNumber);
        void DeleteBranchFromFormerBranchSelection(int customerId, string branchNumber);
        Task DeleteBranchFromFormerBranchSelectionAsync(int customerId, string branchNumber);
        void Finalize(int tourCoverage);
        List<PlanningResult> GetPlanningResult();
        List<PlanningResultDetails> GetPlanningResultDetails();
        int GetCurrentPlanningNumber(DateTime startTime);
        void ExecuteReplaceInsufficientMediaStoredProcedure(decimal tourCoverage);
        void ExecuteFinalResultStoredProcedure(decimal tourCoverage);
        void SavePredefinedBranches(List<string> branchNumbers, int customerId);
        List<PredefinedBranch> GetPredefinedBranches(int customerId);
        void ReleasePredefinedBranchNumbers(List<string> releasables, int customerId);
        List<PlanningDataArea> GetPredefinedArea(string branchNumber);
        Task DeleteFormerBranchSelectionAsync(int customerId);
    }
}
