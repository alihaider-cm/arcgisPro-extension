namespace ArcGisPlannerToolbox.Core.Models;

public class AdvertisementAreaStatistics
{
    public int Filial_ID { get; set; }

    public int Kunden_ID { get; set; }

    public string Kundenname { get; set; }

    public string Vertriebslinie { get; set; }

    public string Filialname { get; set; }

    public int Werbegebiets_Nr { get; set; }

    public string Gebietsbezeichnung { get; set; }

    public string Werbegebietsstatus { get; set; }

    public string gültig_ab_Jahr_KW { get; set; }

    public string gültig_bis_Jahr_KW { get; set; }

    public string Basisgeometrie { get; set; }

    public string Aktionstyp { get; set; }

    public int Gesamtauflage { get; set; }

    public int Anzahl_Medien { get; set; }

    public int ISIS_sichtbar { get; set; }
}
