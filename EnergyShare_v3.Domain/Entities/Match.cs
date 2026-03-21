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
              
        public decimal DistanceCalculée { get; set; } //données calcué ) à partir de la distance entre deux points
        
       
        public Guid VendeurId { get; set; } 
        [ForeignKey("VendeurId")]
        public User Vendeur { get; set; } = null!;

        public Guid AcheteurId { get; set; } //  quid si plusieurs acheteurs cas de figures meme batiments? 
        [ForeignKey("AcheteurId")]
        public User Acheteur { get; set; } = null!;

        // Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
    }
}
