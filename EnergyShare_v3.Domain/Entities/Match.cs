using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class Match
    {
        
        [Key]
        public Guid Id { get; set; }
              
        public decimal DistanceCalculee { get; private set; } //données calcué ) à partir de la distance entre deux points
        
       
        public Guid PointAccessVendeurId { get; set; } 
        [ForeignKey("PointAccessVendeurId")]
        public PointAccess PointAccessVendeur { get; set; } = null!;

        public Guid PointAccessAcheteurId { get; set; }    //  quid si plusieurs acheteurs cas de figures meme batiments? 
        [ForeignKey("PointAccessAcheteurId")]
        public PointAccess PointAccessAcheteur { get; set; } = null!;

        // Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        //Constructeur
        private Match() { } // Constructeur privé pour EF Core
        public Match(Guid pointAccessVendeurId, Guid pointAccessAcheteurId, decimal distanceCalculee)
        {
            PointAccessVendeurId = pointAccessVendeurId;
            PointAccessAcheteurId = pointAccessAcheteurId;
            DistanceCalculee = distanceCalculee;
            VerifierCohérence();
        }

        //Règle de gestion 
        public void VerifierCohérence()
        {
            if (PointAccessVendeurId == PointAccessAcheteurId)
                throw new InvalidOperationException("Un point d'accès ne peut pas être mis en relation avec lui-même.");

            if (DistanceCalculee < 0)
                throw new InvalidOperationException("La distance calculée ne peut pas être négative.");
        }

    }
}
