using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGisPlannerToolbox.Core.Models;

public class CustomerBranch : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int Kunden_ID { get; set; }

    public string Kundenname { get; set; }

    public string Vertriebslinie { get; set; }

    public int Filial_ID { get; set; }

    public string Filial_Nr { get; set; }

    public string Filialname { get; set; }

    public string Straße { get; set; }

    public string PLZ { get; set; }

    public string ORT { get; set; }

    public float X_WGS84 { get; set; }

    public float Y_WGS84 { get; set; }

    public float Entfernung_in_km { get; set; }
    private int _auflage;
    public int Auflage { get { return _auflage; } set { _auflage = value; OnPropertyChanged(); } }

    public int Medium_Id { get; set; }

    public bool VonAlgorithmusGeplant { get; set; }
    public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
