using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Pipes;

namespace EnergyShare_v3.Domain.Entities
{
    public class MembrePartage
    {
        //règle 1 point EAN ne peut appartenir qu'a un seul partage à la fois.
        [Key]
        public Guid Id { get; set; }
        public bool IsInterlocuteurUnique { get; set; } = false;
        public DateTime JoinedAt { get; set; }
        public DateTime? ExitAt { get; set; }
        public DateTime? DateCommunicationPreavis { get; set; }
        public DateTime? DateSortiePlanifiee { get; set; } //peut être calculé à partir de datePravisDonnées + 3 semaines? 

        //Enumérations
        public Guid UserId { get; set; } 
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        public Guid PartageId { get; set; } 
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; } = null!;
        public Guid PointAccessId { get; set; }
        [ForeignKey("PointAccessId")]
        public PointAccess PointAccess { get; set; } = null!;

    
        // Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
    }
}
