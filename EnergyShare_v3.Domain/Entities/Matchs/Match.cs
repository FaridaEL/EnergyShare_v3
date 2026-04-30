using Ardalis.Result;
using EnergyShare_v3.Bricks.Model;
using EnergyShare_v3.Domain.Entities.PointsAccesses;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.Matchs
{
    public class Match :IAuditable
    {    [Key]
        public Guid Id { get; set; }
              
        public decimal DistanceCalculee { get; private set; } //données calcué ) à partir de la distance entre deux points
        
       
        public Guid PointAccessVendeurId { get; set; } 
        [ForeignKey("PointAccessVendeurId")]
        public PointAccess PointAccessVendeur { get; set; } = null!;

        public Guid PointAccessAcheteurId { get; set; }    //  quid si plusieurs acheteurs cas de figures meme batiments? 
        [ForeignKey("PointAccessAcheteurId")]
        public PointAccess PointAccessAcheteur { get; set; } = null!;

        // Données d'audit
        public AuditInfo Audit { get; private set; } = new AuditInfo();

        //Constructeur
        private Match() { } // Constructeur privé pour EF Core
        private Match(Guid pointAccessVendeurId, Guid pointAccessAcheteurId, decimal distanceCalculee)
        {
            PointAccessVendeurId = pointAccessVendeurId;
            PointAccessAcheteurId = pointAccessAcheteurId;
            DistanceCalculee = distanceCalculee;
            
        }

        public static Result<Match> Create(
           Guid pointAccessVendeurId,
           Guid pointAccessAcheteurId,
           decimal distanceCalculee)
                {

                    if (pointAccessVendeurId == Guid.Empty)
                        return MatchErrors.PointAccessVendeurObligatoire().Map();

                    if (pointAccessAcheteurId == Guid.Empty)
                        return MatchErrors.PointAccessAcheteurObligatoire().Map();
                    
                    var match = new Match(pointAccessVendeurId, pointAccessAcheteurId, distanceCalculee);

                    
                    var validation = match.VerifierCoherence();
                    if (!validation.IsSuccess)
                        return Result<Match>.Invalid(validation.ValidationErrors);

                    return Result.Success(match);
        }

        public Result VerifierCoherence()
        {
            if (PointAccessVendeurId == PointAccessAcheteurId)
               return MatchErrors.SameAccessPoint(PointAccessVendeurId, PointAccessAcheteurId);

            if (DistanceCalculee < 0)
                return MatchErrors.DistanceNegative(DistanceCalculee);
            return Result.Success();
        }

    }
}
