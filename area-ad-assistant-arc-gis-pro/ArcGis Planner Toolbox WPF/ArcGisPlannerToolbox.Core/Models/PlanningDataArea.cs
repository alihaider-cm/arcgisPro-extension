namespace ArcGisPlannerToolbox.Core.Models;

public class PlanningDataArea
{
    public int Planungs_Nr { get; set; }
    public int Planungs_ID { get; set; }
    public string Filial_Nr { get; set; }
    public string Geokey { get; set; }
    public string Geokey_Name { get; set; }
    public int HH_Brutto { get; set; }
    public int HH_Netto { get; set; }
    public int Auflage { get; set; }
    public byte[] Geom { get; set; }
}
