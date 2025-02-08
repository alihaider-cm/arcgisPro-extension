namespace ArcGisPlannerToolbox.Core.Models;

public class AddressCicleDetails
{
    public string Hauptfiliale { get; set; }
    public string Nachbarfiliale { get; set; }
    public int Überschneidungen { get; set; }
    public float Anteil_Gesamt { get; set; }
    public float Mittlerer_Anteil { get; set; }
}
