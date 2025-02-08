using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface IGebietsassistentRepository
{
    List<DistributionMedia> GetDestributionMedia();
    Task<List<DistributionMedia>> GetDestributionMediaAsync();
}
