namespace ArcGisPlannerToolbox.Core.Models;

public class PlanningStatistic
{
    public int TotalPlanned { get; set; }
    public double StandardDeviation { get; set; }
    public int NotPlanned { get; set; }
    public int TotalTargetEdition { get; set; }
    public int TotalPlannedEdition { get; set; }
}
