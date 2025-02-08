using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts
{
    public interface IMapViewMediaRepository : IRepository<Media>
    {
        Task<List<int>> FindMediaIdByMapView(List<string> coords);
        List<string> FindIssuesByMapView(List<string> coords, int id);
    }
}
