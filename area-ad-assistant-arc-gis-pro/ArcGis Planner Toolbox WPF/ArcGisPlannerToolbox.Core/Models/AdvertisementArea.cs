using System;

namespace ArcGisPlannerToolbox.Core.Models;

public class AdvertisementArea
{
    public int Werbegebiets_Nr { get; set; }
    public int Filial_ID_Superoffice { get; set; }
    public int Filial_ID { get; set; }
    public int Werbegroßraum_ID { get; set; }
    public string Werbegebietstyp { get; set; }
    public string Werbegebietsstatus { get; set; }
    public string Gebietsbezeichnung { get; set; }
    public string Aktionstyp { get; set; }
    public string Konzeptbezeichnung { get; set; }
    public string gültig_ab_Jahr_KW { get; set; }
    public string gültig_bis_Jahr_KW { get; set; }
    public byte gültig_nur_in_ausgewählten_KW { get; set; }
    public string ausgewählte_KW { get; set; }
    public string Letzte_Änderung_von { get; set; }
    public DateTime Letzte_Änderung_am { get; set; }
    public string Werbegebietsdateipfad { get; set; }
    public string Werbegebietsdateiname { get; set; }
    public string Basisgeometrie { get; set; }
    public DateTime Stand { get; set; }
    public byte ISIS_sichtbar { get; set; }
    public string GBConsiteTitel { get; set; }
    public string GBConsiteURL { get; set; }
    public DateTime GBConsiteURL_erstellt_am { get; set; }
    public DateTime Aktuelles_Datei_Datum { get; set; }
    public int Aktueller_CRC32 { get; set; }
    public int Aktuelle_Dateigröße { get; set; }
    public DateTime Letzte_Verarbeitung_am { get; set; }
    public DateTime Letzte_Verarbeitung_Datei_Datum { get; set; }
    public int Letzte_Verarbeitung_CRC32 { get; set; }
    public int Letzte_Verarbeitung_Dateigröße { get; set; }
    public int Gebietsimport_für_KW { get; set; }
}
