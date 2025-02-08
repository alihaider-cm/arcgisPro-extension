using System;

namespace ArcGisPlannerToolbox.Core.Models
{
    public class Geometry
    {
        public string TourNumber { get; set; }

        public string TourName { get; set; }

        public int NumberOfCopies { get; set; }

        public string NumberOfCopiesInfo { get; set; }

        public int GrossHouseHolds { get; set; }

        public string Appearance { get; set; }

        public int Id { get; set; }

        public int MediaId { get; set; }

        public string MediaName { get; set; }

        public int OccupancyUnitId { get; set; }

        public string DataSource { get; set; }

        public string OccupancyUnit { get; set; }

        public string NameTitle { get; set; }

        public string IssueNumber { get; set; }

        public string Issue { get; set; }

        public int TourId { get; set; }

        public DateTime DataStatus { get; set; }

        public DateTime CreationDate { get; set; }

        public int NumberOfGeometries { get; set; }

        public int NumberOfFaultyGeometries { get; set; }

        public DateTime CleaningDate { get; set; }

        public string GeographyString { get; set; }

        public byte[] Geom { get; set; }
    }
}
