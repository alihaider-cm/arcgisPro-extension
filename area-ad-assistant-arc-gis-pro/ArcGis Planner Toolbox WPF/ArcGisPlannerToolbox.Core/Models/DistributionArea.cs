using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace ArcGisPlannerToolbox.Core.Models
{
    [Table("VERBREITUNGSGEBIETE")]
    public class DistributionArea
    {
        public int ID { get; set; }

        [Column("DATENQUELLE")]
        public string DataSource { get; set; }

        [Column("VERBREITUNGSGEBIET_MEDIUM_ID")]
        public int Media_ID { get; set; }

        [Column("VERBREITUNGSGEBIET_PLZ")]
        public string ZipCode { get; set; }

        [Column("VERBREITUNGSGEBIET_GKZ")]
        public string CommunityCode { get; set; } //Gemeindekennziffer

        [Column("VERBREITUNGSGEBIET_STREUART")]
        public string SpreadingType { get; set; }

        [Column("ZIS_NR")]
        public int ZIS_No { get; set; }

        [Column("LKZ")]
        public string CountryCode { get; set; }

        [Column("AUFLAGE")]
        public int? NumberOfCopies { get; set; }

        [Column("AUSGABE")]
        public string Issue { get; set; }

        [Column("TOURENBEZEICHNUNG")]
        public string TourName { get; set; }

        [Column("GEMEINDE")]
        public string Community { get; set; }

        [Column("ORTSTEIL")]
        public string District { get; set; }

        [Column("LIEFERANSCHRIFTEN_ID")]
        public int? DeliveryAdress_ID { get; set; }

        [Column("TRÄGERSTRUKTUR")]
        public string SupportStructure { get; set; }

        [Column("TITEL")]
        public string Title { get; set; }

        [Column("KOPFBLATT")]
        public string HeadSheet { get; set; }

        [Column("WV_BEDIENEN")]
        public string AdvertisingObjectorsServing { get; set; } //Werbeverweiberer

        [Column("WV_BEDIENEN_AEND_AM")]
        public DateTime? AdvertisingObjectorsServingOn { get; set; }

        [Column("WV_BEDIENEN_AEND_VON")]
        public string AdvertisingObjectorsServingFrom { get; set; }

        [Column("VERBREITUNGSGEBIET_MONTAGS")]
        public string Mondays { get; set; }

        [Column("VERBREITUNGSGEBIET_DIENSTAGS")]
        public string Tuesdays { get; set; }

        [Column("VERBREITUNGSGEBIET_MITTWOCHS")]
        public string Wednesdays { get; set; }

        [Column("VERBREITUNGSGEBIET_DONNERSTAGS")]
        public string Thursdays { get; set; }

        [Column("VERBREITUNGSGEBIET_FREITAGS")]
        public string Fridays { get; set; }

        [Column("VERBREITUNGSGEBIET_SAMSTAGS")]
        public string Saturdays { get; set; }

        [Column("VERBREITUNGSGEBIET_SONNTAGS")]
        public string Sundays { get; set; }

        [Column("VERBREITUNGSGEBIET_Erscheinweise")]
        public string Appearance { get; set; }

        [Column("BEMERKUNG")]
        public string Comment { get; set; }

        [Column("ERSTELLT_AM")]
        public DateTime? CreatedOn { get; set; }

        [Column("ERSTELLT_VON")]
        public string CreatedFrom { get; set; }

        [Column("LETZTE_AENDERUNG_AM")]
        public DateTime? LatestChangeOn { get; set; }

        [Column("LETZTE_AENDERUNG_VON")]
        public string LatestChangeFrom { get; set; }

        [Column("Stand")]
        public DateTime? Status { get; set; }
    }
}
