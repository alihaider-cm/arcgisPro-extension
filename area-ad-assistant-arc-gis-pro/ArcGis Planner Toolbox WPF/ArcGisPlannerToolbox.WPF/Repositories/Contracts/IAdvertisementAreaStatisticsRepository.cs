using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface IAdvertisementAreaStatisticsRepository : IRepository<AdvertisementAreaStatistics>
{
    List<ActionType> GetActionTypes(int customerId);
    List<AdvertisementAreaStatistics> GetCustomerStatisticsByBranch(CustomerBranch branch);
    void SetDataBase(string database);
}
