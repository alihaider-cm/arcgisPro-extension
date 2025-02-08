using System;

namespace ArcGisPlannerToolbox.Core.Models;

public class OccupancyUnit
{
    public string ZipCode { get; set; }

    public string MicroZipCode { get; set; }

    public string ZipCodeName { get; set; }

    public string Districts { get; set; }

    public string DistrictHousholdsProportion { get; set; }

    public int AdvertisingObjectorsQuote { get; set; }

    public int GrossHousehold { get; set; }

    public int NetHouesehold { get; set; }

    public int Residents { get; set; }

    public string CommutityCode { get; set; }

    public string CommunityName { get; set; }

    public DateTime LastModified { get; set; }

    public DateTime BorderStatus { get; set; }

    public string Author { get; set; }

    public string EvaluatedFrom { get; set; }

    public DateTime EvaluatedOn { get; set; }

    //public string TourDistance { get; set; }
}
