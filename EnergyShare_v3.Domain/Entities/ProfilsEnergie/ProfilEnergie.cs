using Ardalis.Result;
using EnergyShare_v3.Bricks.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.ProfilsEnergie
{
    public class ProfilEnergie : IAuditable
    {
        /* utilisé pour le matching et la simulation de l'économie d'énergie réalisée grâce au partage d'énergie,
         * en comparant le prix d'achat cible de l'acheteur avec le prix de vente cible du vendeur
         en comparant le prix de vente cible du vendeur avec le prix d'achat auprès de son fournisseur d'énergie actuel*/
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? DemandeEnergie_kWh { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? OffreEnergie_kWh { get; set; }
        public decimal? PrixAchatCible_Eur { get; set; }
        public decimal? PrixVenteCible_Eur { get; set; }
        public Guid PointAccessId { get; set; }

        [ForeignKey("PointAccessId")]
        public PointAccess PointAccess { get; set; } = null!;

        // Audit
        public AuditInfo Audit { get; private set; } = new AuditInfo();

        // Constructeur EF
        public ProfilEnergie() { }

        // Constructeur métier utilisé par Create()
        private ProfilEnergie(
            decimal? demande,
            decimal? offre,
            decimal? prixAchatCible,
            decimal? prixVenteCible,
            Guid pointAccessId)
        {
            Id = Guid.NewGuid();
            PointAccessId = pointAccessId;
            DemandeEnergie_kWh = demande;
            OffreEnergie_kWh = offre;
            PrixAchatCible_Eur = prixAchatCible;
            PrixVenteCible_Eur = prixVenteCible;
        }

        public static Result<ProfilEnergie> Create(
            decimal? demande,
            decimal? offre,
            decimal? prixAchatCible,
            decimal? prixVenteCible,
            Guid pointAccessId)
        {
            // règle 1 : offre ou demande obligatoire
            if (!demande.HasValue && !offre.HasValue)
                return ProfilEnergieErrors.OffreOuDemandeRequise().Map();

            // règle 2 : valeurs négatives interdites
            if (demande.HasValue && demande.Value < 0)
                return ProfilEnergieErrors.ValeurNegative(nameof(demande)).Map();

            if (offre.HasValue && offre.Value < 0)
                return ProfilEnergieErrors.ValeurNegative(nameof(offre)).Map();

            if (prixAchatCible.HasValue && prixAchatCible.Value < 0)
                return ProfilEnergieErrors.ValeurNegative(nameof(prixAchatCible)).Map();

            if (prixVenteCible.HasValue && prixVenteCible.Value < 0)
                return ProfilEnergieErrors.ValeurNegative(nameof(prixVenteCible)).Map();

            // règle 3 : pointAccess obligatoire
            if (pointAccessId == Guid.Empty)
                return Result<ProfilEnergie>.Invalid(new ValidationError(
                    nameof(pointAccessId),
                    "Le point d'accès est obligatoire",
                    "ProfilEnergie.PointAccessObligatoire",
                    ValidationSeverity.Error));

            return Result.Success(new ProfilEnergie(
                demande,
                offre,
                prixAchatCible,
                prixVenteCible,
                pointAccessId));
        }

    }
   
}
