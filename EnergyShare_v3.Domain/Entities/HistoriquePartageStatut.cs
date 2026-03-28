using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class HistoriquePartageStatut
    {
        /*Le statut d'un partage peut évoluer inactif, en attente de validation, etc. */
           [Key]
        public Guid Id { get; set; }

        //enumération
        [Required]
        public PartageEnergieStatutType Statut { get; set; }
        public PartageEnergieStatutType? OldStatut { get; set; }
        public string? Motif { get; set; }

        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; }

        

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }


}
