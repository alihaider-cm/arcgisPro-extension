using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface IAdvertisementAreaGeometryRepository : IRepository<AdvertisementAreaGeometry>
{
    List<AdvertisementAreaGeometry> GetAdvertisementAreaGeometreiysByNumbers(List<int> advertisementAreaNumbers);
    List<AdvertisementAreaGeometry> GetAdvertisementAreaGeometryByNumber(int advertisementAreaNumber);
}
