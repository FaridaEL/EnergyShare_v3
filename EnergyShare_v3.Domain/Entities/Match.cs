using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Pipes;

namespace EnergyShare_v3.Domain.Entities
{
    public class Match
    {
        
        [Key]
        public Guid Id { get; set; }
              
        public decimal DistanceCalculee { get; set; } //données calcué ) à partir de la distance entre deux points
        
       
        public Guid PointAccessVendeurId { get; set; } 
        [ForeignKey("PointAccessVendeurId")]
        public PointAccess PointAccessVendeur { get; set; } = null!;

        public Guid PointAccesAcheteurId { get; set; } //  quid si plusieurs acheteurs cas de figures meme batiments? 
        [ForeignKey("PointAccesAcheteurId")]
        public PointAccess PointAccesAcheteur { get; set; } = null!;

        // Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
    }
}
