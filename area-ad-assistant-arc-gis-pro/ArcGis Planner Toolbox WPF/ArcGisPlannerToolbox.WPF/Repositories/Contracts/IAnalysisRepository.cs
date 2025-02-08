using ArcGisPlannerToolbox.Core.Models;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts;

public interface IAnalysisRepository
{
    List<Analysis> GetAnalysisByCustomerId(int customerId);
}
