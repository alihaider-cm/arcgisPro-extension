using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;
using System.Data.Spatial;


namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts
{
    public interface IGeometryRepository : IRepository<Geometry>
    {
       DbGeography GetSinglePolygon(List<string> zipCodes, string mediaName);
    }
}
