namespace ArcGisPlannerToolbox.Core.Models;

public class Customer
{
    public int Kunden_ID { get; set; }

    public string Kunde { get; set; }

    public string Firmenname { get; set; }

    public string Straße { get; set; }

    public string PLZ { get; set; }

    public string ORT { get; set; }

    public string Bundesland { get; set; }

    public string Kunden_Nr { get; set; }

    public string Organisations_Nr { get; set; }

    public int Adress_Id { get; set; }

    public int ISIS_aktiv { get; set; }

    public float X_GK3 { get; set; }

    public float Y_GK3 { get; set; }
}
