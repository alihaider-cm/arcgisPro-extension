using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcGisPlannerToolbox.Core.Models;

[Table("MEDIEN")]
public class Media
{
    [Column("MEDIUM_ID")]
    public int Id { get; set; }

    [Column("MEDIUM_NAME")]
    public string Name { get; set; }

    [Column("MEDIUM_TYP")]
    public string MediaType { get; set; }

    [Column("STREUART")]
    public string MediaSpreadingType { get; set; }

    [Column("AKTIV")]
    public bool IsActive { get; set; }

    [Column("MEDIUM_ERSCHEINWEISE")]
    public AppearanceRhythm AppearanceRhythm { get; set; }

    [Column("MEDIUM_ERSCHEINT_WOCHENANFANG")]
    public bool AppersBeginningOfWeek { get; set; }

    [Column("MEDIUM_ERSCHEINT_WOCHENMITTE")]
    public bool AppersMiddleOfWeek { get; set; }

    [Column("MEDIUM_ERSCHEINT_WOCHENENDE")]
    public bool AppersEndOfWeek { get; set; }

    [Column("Verbreitungsgebiet_Datenquelle")]
    public string DistributionAreaSource { get; set; }

    [Column("Verbreitungsgebiet_Letzte_Änderung_am")]
    public DateTime? Status { get; set; }

    [Column("KLEINSTE_BELEGUNGSEINHEIT")]
    public string SmallestOccupancyUnit { get; set; }

    public List<DistributionArea> Area { get; set; }

    [NotMapped]
    public bool HasDistributionArea { get; set; }
}
