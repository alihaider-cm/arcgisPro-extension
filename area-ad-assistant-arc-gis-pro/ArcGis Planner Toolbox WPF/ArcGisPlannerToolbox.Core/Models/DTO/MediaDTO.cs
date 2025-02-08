using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGisPlannerToolbox.Core.Models.DTO;

public class MediaDTO : Media, INotifyPropertyChanged
{
	private bool _booleanValue = false;
	public bool BooleanValue
	{
		get { return _booleanValue; }
		set { _booleanValue = value; OnPropertyChanged(); }
	}

    public MediaDTO()
    {
        
    }
    public MediaDTO(Media media)
    {
        Id = media.Id;
        Name = media.Name;
        MediaType = media.MediaType;
        MediaSpreadingType = media.MediaSpreadingType;
        AppearanceRhythm = media.AppearanceRhythm;
        AppersBeginningOfWeek = media.AppersBeginningOfWeek;
        AppersEndOfWeek = media.AppersEndOfWeek;
        AppersMiddleOfWeek = media.AppersMiddleOfWeek;
        Area = media.Area;
        Status = media.Status;
        IsActive = media.IsActive;
        DistributionAreaSource = media.DistributionAreaSource;
        HasDistributionArea = media.HasDistributionArea;
        SmallestOccupancyUnit = media.SmallestOccupancyUnit;
    }
    public MediaDTO(Media media, bool booleanValue)
    {
        Id = media.Id;
        Name = media.Name;
        MediaType = media.MediaType;
        MediaSpreadingType = media.MediaSpreadingType;
        AppearanceRhythm = media.AppearanceRhythm;
        AppersBeginningOfWeek = media.AppersBeginningOfWeek;
        AppersEndOfWeek = media.AppersEndOfWeek;
        AppersMiddleOfWeek = media.AppersMiddleOfWeek;
        Area = media.Area;
        Status = media.Status;
        IsActive = media.IsActive;
        DistributionAreaSource = media.DistributionAreaSource;
        HasDistributionArea = media.HasDistributionArea;
        SmallestOccupancyUnit = media.SmallestOccupancyUnit;
        BooleanValue = booleanValue;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
