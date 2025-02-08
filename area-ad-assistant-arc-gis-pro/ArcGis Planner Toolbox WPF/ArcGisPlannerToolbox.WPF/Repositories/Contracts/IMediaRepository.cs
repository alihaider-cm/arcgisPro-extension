using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGisPlannerToolbox.Core.Models;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface IMediaRepository : IRepository<Media>
{
    List<Media> GetAllMedia();
    Task<List<Media>> GetAllMediaAsync();
    List<Media> FindByNamePattern(string namePattern);
    List<Media> FindContinuedByID(int idPattern);
    Media GetWithDistributionArea(int id);
    Task<Media> GetWithDistributionAreaAsync(int id);
    Media GetWithDistributionArea(int id, string dataSource);
    Task<List<int>> FindMediaIdByLocation(string location);
    List<string> GetAvailableMediaTypes(List<string> selSpreadingTyps, List<string> selTimeOfAppearance);
    List<string> GetAvailableSpreadingTypes(List<string> selMediaTyps, List<string> selTimeOfAppearance);
    List<string> GetAvailableTimesOfAppearance(List<string> selMediaTyps, List<string> selSpreadingTyps);
    Task<List<string>> GetAvailableTimesOfAppearanceAsync(List<string> selMediaTyps, List<string> selSpreadingTyps);
    List<string> GetAvailableAppearanceRhythm(List<string> selMediaTypes, List<string> selSpreadingTypes);
    List<SmallestOccUnit> GetSmallestOccUnitsList();
    void UpdateMediasSmallesOccUnit(int id, string newOccUnit);
    List<Media> FindAllByID(int idPattern);
    List<MediaDigitizationState> GetStateOfDigitization(string spreadingType = null);
    Task<IEnumerable<MediaDigitizationState>> GetStateOfDigitizationAsync(string spreadingType = null);
}
