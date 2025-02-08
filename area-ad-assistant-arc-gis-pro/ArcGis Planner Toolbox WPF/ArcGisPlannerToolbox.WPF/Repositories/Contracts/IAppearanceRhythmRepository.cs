using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface IAppearanceRhythmRepository : IRepository<AppearanceRhythm>
{
    List<AppearanceRhythm> GetAllMedaRhythms();
    Task<List<AppearanceRhythm>> GetAllMedaRhythmsAsync();
}
