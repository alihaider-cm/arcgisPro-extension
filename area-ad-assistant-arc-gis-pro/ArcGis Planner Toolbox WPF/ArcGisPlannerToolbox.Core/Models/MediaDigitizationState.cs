using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcGisPlannerToolbox.Core.Models;

public class MediaDigitizationState
{

    [Column("MEDIUM_ID")]
    public int MEDIUM_ID { get; set; }

    public string PLZ_Scharf { get; set; }

    [Column("AKTIV")]
    public int AKTIV { get; set; }

    [Column("TEST_MEDIUM")]
    public int TEST_MEDIUM { get; set; }

    [Column("EINGESTELLT")]
    public int EINGESTELLT { get; set; }

    [Column("MEDIUM_NAME")]
    public string MEDIUM_NAME { get; set; }

    [Column("MEDIUM_TYP")]
    public string MEDIUM_TYP { get; set; }

    [Column("STREUART")]
    public string STREUART { get; set; }

    [Column("MEDIENKATEGORIE")]
    public string MEDIENKATEGORIE { get; set; }

    [Column("FIRMEN_ID")]
    public int FIRMEN_ID { get; set; }

    [Column("FIRMA")]
    public string FIRMA { get; set; }

    [Column("FIRMA_LKZ")]
    public string FIRMA_LKZ { get; set; }

    [Column("FIRMA_PLZ")]
    public string FIRMA_PLZ { get; set; }

    [Column("FIRMA_ORT")]
    public string FIRMA_ORT { get; set; }

    [Column("FIRMA_ADRESSE")]
    public string FIRMA_ADRESSE { get; set; }

    [Column("ERSCHEINRHYTHMUS")]
    public string ERSCHEINRHYTHMUS { get; set; }

    [Column("ERSCHEINTAGE")]
    public string ERSCHEINTAGE { get; set; }

    [Column("KLEINSTE_BELEGUNGSEINHEIT")]
    public string KLEINSTE_BELEGUNGSEINHEIT { get; set; }

    [Column("MINDESTAUFLAGE")]
    public int MINDESTAUFLAGE { get; set; }

    [Column("MINDESTBESTELLWERT")]
    public int MINDESTBESTELLWERT { get; set; }

    [Column("GEBIET_DATENQUELLE")]
    public string GEBIET_DATENQUELLE { get; set; }

    [Column("GEBIET_STATUS")]
    public string GEBIET_STATUS { get; set; }

    [Column("STAND_GEBIET")]
    public DateTime STAND_GEBIET { get; set; }

    [Column("STAND_AUFLAGE")]
    public DateTime STAND_AUFLAGE { get; set; }

    [Column("GEBIET_AUFLAGE")]
    public int GEBIET_AUFLAGE { get; set; }

    [Column("MEDIUM_IM_KUNDEN_EINSATZ")]
    public string MEDIUM_IM_KUNDEN_EINSATZ { get; set; }

    [Column("SCHALTAUFLAGE")]
    public int SCHALTAUFLAGE { get; set; }

    [Column("TEILWEISE_DIGITALISIERT")]
    public int TEILWEISE_DIGITALISIERT { get; set; }

    [Column("DIGITALISIERT_VON")]
    public string DIGITALISIERT_VON { get; set; }

    [Column("DIGITALISIERT_ZULETZT_AM")]
    public DateTime DIGITALISIERT_ZULETZT_AM { get; set; }

    [Column("EBENEN_GESAMT")]
    public int EBENEN_GESAMT { get; set; }

    [Column("EBENEN_DIGITALISIERT")]
    public int EBENEN_DIGITALISIERT { get; set; }

    [Column("EBENEN_SYSTEM_DIGITALISIERT")]
    public int EBENEN_SYSTEM_DIGITALISIERT { get; set; }

    [Column("EBENEN_DIGITALISIERT_PROZENT")]
    public float EBENEN_DIGITALISIERT_PROZENT { get; set; }

    [Column("EBENE_BEZEICHNUNG")]
    public string EBENE_BEZEICHNUNG { get; set; }

    [Column("ARBEITSSTATUS")]
    public int ARBEITSSTATUS { get; set; }
}
