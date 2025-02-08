using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface IAdvertisementAreaRepository : IRepository<AdvertisementArea>
{
    bool CheckIfAdvertisementAreaDataExists(string name, int branchNumber);
    List<AdvertisementAreaAreas> GetAdvertisementAreaAreasByOccupancyUnitLevel(int advertisementAreaNumber);
    List<AdvertisementAreaAreas> GetAdvertisementAreaAreasByZipCodeLevel(int advertisementAreaNumber);
    void SaveAdvertisementArea(AdvertisementArea advertisementArea, List<int> areaUnits, List<AdvertisementArea> neighborBranchAreas = null, bool updateData = false);
    void SaveAdvertisementArea(AdvertisementArea advertisementArea, Dictionary<string, int> zipCodeUnits, List<AdvertisementArea> neighborBranchAreas = null, bool updateData = false);
    void SaveAdvertisementArea(AdvertisementArea advertisementArea, Dictionary<string, int> zipCodeUnits, List<int> areaUnits, List<AdvertisementArea> neighborBranchAreas = null, bool updateData = false);
}
