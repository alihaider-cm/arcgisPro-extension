using System;

namespace ArcGisPlannerToolbox.Core.Models;

public class PlanningData
{
    public int Planungs_Nr { get; set; }
    public int Konzept_Nr { get; set; }
    public int Analyse_Id { get; set; }
    public int Kunden_ID { get; set; }
    public string Kunden_Name { get; set; }
    public string Filial_Nr { get; set; }
    public string Planungstyp { get; set; }
    public int Zielauflage1 { get; set; }
    public int Zielauflage2 { get; set; }
    public int Zielauflage3 { get; set; }
    public int Mindestauflage { get; set; }
    public int Maximalauflage { get; set; }
    public DateTime Stand { get; set; }
    public int HH_Brutto { get; set; }
    public int HH_Netto { get; set; }
    public int Auflage { get; set; }
    public DateTime Planungsbeginn { get; set; }
    public DateTime Planungsende { get; set; }
    public string Planung_von { get; set; }
}
