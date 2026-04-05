using EnergyShare_v3.Domain.Entities.Partages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class DataPartage 
    {
            /*Doit correspondre aux champs du fichier sibelga qui est envoyé chaque mois avec les données réelles du partage
             Servira aussi au Dashboard statistique
            Dans ce MVP, je simplifie volontairement les champs car je ne traite pas de la facturation*/
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage PartageEnergie { get; set; } = null!;

        [Required]
        public DateTime DateDebut { get; set; }     //FromDate  // Identifiant de la période (souvent un mois ou un quart d'heure selon  besoin)
        [Required]
        public DateTime DateFin { get; set; }      //ToDate

        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? VolumePartage_kWh { get; set; } // "Volume local" dans le fichier Sibelga


        // Métadonnées d'importation
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    }
    
}
