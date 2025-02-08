using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories;

/// <summary>
/// This is the repository for interaction with appearance rhythm data.
/// </summary>
public class AppearanceRhythmRepository : Repository<AppearanceRhythm>, IAppearanceRhythmRepository
{
    public AppearanceRhythmRepository(IDbContext dbContext) : base(dbContext, DbConnectionName.MediaCenter)
    {
    }

    /// <summary>
    /// GetAllMedaRhythms() returns a list of all the rhythms in the database
    /// </summary>
    /// <returns>
    /// A list of all the MedaRhythms in the database.
    /// </returns>
    public List<AppearanceRhythm> GetAllMedaRhythms()
    {
        return GetAll().ToList();
    }

    public async Task<List<AppearanceRhythm>> GetAllMedaRhythmsAsync()
    {
        return GetAll().ToList();
    }

}
