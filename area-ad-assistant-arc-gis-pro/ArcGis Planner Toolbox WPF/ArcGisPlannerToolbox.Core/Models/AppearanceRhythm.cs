using System.ComponentModel.DataAnnotations.Schema;

namespace ArcGisPlannerToolbox.Core.Models
{
    [Table("MEDIEN_ERSCHEINRHYTHMUS")]
    public class AppearanceRhythm
    {
        [Column("RH_ID")]
        public int Id { get; set; }

        [Column("BEZEICHNUNG")]
        public string Name { get; set; }
    }
}
