using System;

namespace ArcGisPlannerToolbox.Core.Models
{
    public class AdvertisementAreaGeometry
    {
        public int Werbegebiets_nr { get; set; }

        public int Medium_id { get; set; }

        public DateTime Stand { get; set; }

        public DateTime Datenstand { get; set; }

        public int Anzahl_geoschluessel { get; set; }

        public DateTime Generiert_am { get; set; }

        public int Generiert_in_ms { get; set; }

        public byte[] Geom { get; set; }

        public int Aktives_gebiet_kunden_id { get; set; }

        public int Aktives_gebiet_reihenfolge { get; set; }

        public int Aktives_gebiet_style_id { get; set; }

        public int HH_brutto { get; set; }

        public int Ew { get; set; }

        public float KK_idx { get; set; }

        public int Berechnungsmethode { get; set; }

        public int Geom_is_valid { get; set; }
    }
}
