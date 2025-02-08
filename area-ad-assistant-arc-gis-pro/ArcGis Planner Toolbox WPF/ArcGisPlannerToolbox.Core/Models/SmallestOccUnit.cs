namespace ArcGisPlannerToolbox.Core.Models;

public class SmallestOccUnit
{
    public SmallestOccUnit()
    {
    }

    public int Id { get; set; }

    public string OccupancyUnit { get; set; }

    public int Ordered { get; set; }

    public bool IsActive { get; set; }
}
