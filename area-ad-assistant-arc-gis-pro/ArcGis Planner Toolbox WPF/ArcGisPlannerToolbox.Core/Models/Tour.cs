using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGisPlannerToolbox.Core.Models;

public class Tour : INotifyPropertyChanged
{
    private int id;
    private int? occupancyUnitId;
    private string mediaId;
    private string issue;
    private string tourName;
    private string tourId;
    private string tourNumber;
    private string zipCode;
    private string location;
    private string district;
    private int printNumber;
    private string author;
    private string distanceToOccupancyUnit;

    public int Id { get => id; set { id = value; OnPropertyChanged(); } }

    public int? OccupancyUnitId { get => occupancyUnitId; set { occupancyUnitId = value; OnPropertyChanged(); } }

    public string MediaId { get => mediaId; set { mediaId = value; OnPropertyChanged(); } }

    public string Issue { get => issue; set { issue = value; OnPropertyChanged(); } }

    public string TourName { get => tourName; set { tourName = value; OnPropertyChanged(); } }

    public string TourId { get => tourId; set { tourId = value; OnPropertyChanged(); } }

    public string TourNumber { get => tourNumber; set { tourNumber = value; OnPropertyChanged(); } }

    public string ZipCode { get => zipCode; set { zipCode = value; OnPropertyChanged(); } }

    public string Location { get => location; set { location = value; OnPropertyChanged(); } }

    public string District { get => district; set { district = value; OnPropertyChanged(); } }

    public int PrintNumber { get => printNumber; set { printNumber = value; OnPropertyChanged(); } }

    public string Author { get => author; set { author = value; OnPropertyChanged(); } }

    public string DistanceToOccupancyUnit { get => distanceToOccupancyUnit; set { distanceToOccupancyUnit = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
