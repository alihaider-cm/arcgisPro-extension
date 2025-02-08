using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGisPlannerToolbox.Core.Models.DTO;

public class OccupancyUnitDTO : OccupancyUnit, INotifyPropertyChanged
{
    private bool _isChecked;
    public bool IsChecked
    {
        get { return _isChecked; }
        set { _isChecked = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public OccupancyUnitDTO()
    {

    }
    public OccupancyUnitDTO(OccupancyUnit occupancyUnit)
    {
        AdvertisingObjectorsQuote = occupancyUnit.AdvertisingObjectorsQuote;
        Author = occupancyUnit.Author;
        BorderStatus = occupancyUnit.BorderStatus;
        CommunityName = occupancyUnit.CommunityName;
        CommutityCode = occupancyUnit.CommutityCode;
        DistrictHousholdsProportion = occupancyUnit.DistrictHousholdsProportion;
        Districts = occupancyUnit.Districts;
        EvaluatedFrom = occupancyUnit.EvaluatedFrom;
        EvaluatedOn = occupancyUnit.EvaluatedOn;
        GrossHousehold = occupancyUnit.GrossHousehold;
        LastModified = occupancyUnit.LastModified;
        MicroZipCode = occupancyUnit.MicroZipCode;
        NetHouesehold = occupancyUnit.NetHouesehold;
        Residents = occupancyUnit.Residents;
        ZipCode = occupancyUnit.ZipCode;
        ZipCodeName = occupancyUnit.ZipCodeName;
        IsChecked = false;
    }
    public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
