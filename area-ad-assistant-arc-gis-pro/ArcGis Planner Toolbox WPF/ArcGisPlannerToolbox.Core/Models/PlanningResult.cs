namespace ArcGisPlannerToolbox.Core.Models;

public class PlanningResult
{
    public string Filial_Nr { get; set; }
    public string PLZ { get; set; }
    public int Medium_ID { get; set; }
    public string Medium_Name { get; set; }
    public int HH_Brutto { get; set; }
    public int HH_Netto { get; set; }
    public int Auflage_Filiale { get; set; }
    public int Auflage_PLZ { get; set; }
    public string PLZ_Schärfe { get; set; }
    public string PLZ_Schärfe_Filiale { get; set; }
    public decimal Reichweite { get; set; }
    public decimal Reichweite_Post { get; set; }
    public string Tour { get; set; }
    public int Tourenauflage { get; set; }
    public decimal Tourenabdeckung { get; set; }
    public string Belegung_der_PLZ { get; set; }
    public int Anzahl_Touren_in_PLZ { get; set; }
    public string Belegung_der_Tour { get; set; }
    public string Datenquelle { get; set; }
    public string Ebene { get; set; }
    public string Info { get; set; }
    public string Status { get; set; }
}
