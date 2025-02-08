using ArcGIS.Core.Geometry;

namespace ArcGisPlannerToolbox.WPF.Data;

public class PolygonGeometry : ArcGisPlannerToolbox.Core.Models.Geometry
{
    public PolygonGeometry()
    {
        
    }
    public PolygonGeometry(ArcGisPlannerToolbox.Core.Models.Geometry geometry)
    {
        Id = geometry.Id;
        CreationDate = geometry.CreationDate;
        CleaningDate = geometry.CleaningDate;
        CleaningDate = geometry.CleaningDate;
        DataSource = geometry.DataSource;
        DataStatus = geometry.DataStatus;
        GeographyString = geometry.GeographyString;
        Geom = geometry.Geom;
        GrossHouseHolds = geometry.GrossHouseHolds;
        Issue = geometry.Issue;
        IssueNumber = geometry.IssueNumber;
        MediaId = geometry.MediaId;
        MediaName = geometry.MediaName;
        NameTitle = geometry.NameTitle;
        NumberOfCopies = geometry.NumberOfCopies;
        NumberOfCopiesInfo = geometry.NumberOfCopiesInfo;
        NumberOfFaultyGeometries = geometry.NumberOfFaultyGeometries;
        NumberOfGeometries = geometry.NumberOfGeometries;
        OccupancyUnit = geometry.OccupancyUnit;
        OccupancyUnitId = geometry.OccupancyUnitId;
        TourId = geometry.TourId;
        TourName = geometry.TourName;
        TourNumber = geometry.TourNumber;
        Appearance = geometry.Appearance;
    }
    public Polygon Polygon { get; set; }
    public int ObjectId { get; set; }
}
