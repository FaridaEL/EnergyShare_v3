using Ardalis.Result;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.ProfilsEnergie
{
    public class ProfilEnergie
    {
        /* utilisé pour le matching et la simulation de l'économie d'énergie réalisée grâce au partage d'énergie,
         * en comparant le prix d'achat cible de l'acheteur avec le prix de vente cible du vendeur
         en comparant le prix de vente cible du vendeur avec le prix d'achat auprès de son fournisseur d'énergie actuel*/
        [Key]    
        public Guid Id { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? DemandeEnergie_kWh { get; set; } // Qt d'énergie que l'acheteur souhaite acheter en kWh
        [Column(TypeName = "decimal(18,4)")]
        public decimal? OffreEnergie_kWh { get; set; } //Qt d'énergie que le vendeur souhaite vendre en kWh
             
        public decimal? PrixAchatCible_Eur { get; set; }  //Prix d'achat cible que l'acheteur est prêt à payer en €/kWh, utilisé pour les algorithmes de matching et de tarification dynamique

        public decimal? PrixVenteCible_Eur { get; set; }  //Prix de vente auquel le vendeur est prêt à vendre en cent €/kWh, utilisé pour les algorithmes de matching et de tarification dynamique
        
        public decimal? ConsommationAnnuelleEstime_kWh { get; set; }
        public decimal? ProductionAnnuelleEstime_kWh { get; set; }
        public decimal? PrixAchatEnergieFournisseur_Eur { get; set; } //Prix d'achat auprès de son fournisseur d'énergie actuel en €/kWh, 
        public decimal? PrixVenteInjectionFournisseurActuel_Eur {  get; set; } //permet de calculer l'écononmie
        
        [Required] //rgèle métier : en créant un profil l'utlisateur donne son consentement pour le partage de ses données.
        public bool AccordConsentement { get; private set; } = true;
        public DateTime DateAccordConsentement { get; private set; } = DateTime.UtcNow;
        public DateTime? DateRetraitConsentement { get;  private set; } 

        public Guid PointAccessId { get; set; }  
        [ForeignKey("PointAccessId")]
        public PointAccess PointAccess { get; set; } = null!;

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //Constructeurs
        public ProfilEnergie() { }    //constructeur par défaut nécessaire pour EF Core
                                      //constructeur pour la création d'un profil énergie, le consentement est donné par défaut

        // Constructeur privé métier (optionnel ici)
        // Il est actuellement non utilisé (grisé) car la création se fait via Create(...) avec un object initializer.
        // Contrairement à Partage, on ne passe pas encore par ce constructeur.
        // Peut être utilisé plus tard si la création devient plus complexe.
        private ProfilEnergie(
            decimal? demandeEnergie_kWh,
            decimal? offreEnergie_kWh,
            decimal? prixAchatCible_Eur,
            decimal? prixVenteCible_Eur,
            decimal? consommationAnnuelleEstime_kWh,
            decimal? productionAnnuelleEstime_kWh,
            decimal? prixAchatEnergieFournisseur_Eur,
            decimal? prixVenteInjectionFournisseurActuel_Eur,
            Guid pointAccessId)
        {
            Id = Guid.NewGuid();
            PointAccessId = pointAccessId;
            DemandeEnergie_kWh = demandeEnergie_kWh;
            OffreEnergie_kWh = offreEnergie_kWh;
            PrixAchatCible_Eur = prixAchatCible_Eur;
            PrixVenteCible_Eur = prixVenteCible_Eur;
            ConsommationAnnuelleEstime_kWh = consommationAnnuelleEstime_kWh;
            ProductionAnnuelleEstime_kWh = productionAnnuelleEstime_kWh;
            PrixAchatEnergieFournisseur_Eur = prixAchatEnergieFournisseur_Eur;
            PrixVenteInjectionFournisseurActuel_Eur = prixVenteInjectionFournisseurActuel_Eur;
            AccordConsentement = true;
            DateAccordConsentement = DateTime.UtcNow;
            DateRetraitConsentement = null;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public static Result<ProfilEnergie> Create(
        decimal? demande,
        decimal? offre,
        decimal? prixAchatCible,
        decimal? prixVenteCible,
        decimal? consommationAnnuelle,
        decimal? productionAnnuelle,
        decimal? prixAchatFournisseur,
        decimal? prixVenteInjection,
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

            if (consommationAnnuelle.HasValue && consommationAnnuelle.Value < 0)
                return ProfilEnergieErrors.ValeurNegative(nameof(consommationAnnuelle)).Map();

            if (productionAnnuelle.HasValue && productionAnnuelle.Value < 0)
                return ProfilEnergieErrors.ValeurNegative(nameof(productionAnnuelle)).Map();

            if (prixAchatFournisseur.HasValue && prixAchatFournisseur.Value < 0)
                return ProfilEnergieErrors.ValeurNegative(nameof(prixAchatFournisseur)).Map();

            if (prixVenteInjection.HasValue && prixVenteInjection.Value < 0)
                return ProfilEnergieErrors.ValeurNegative(nameof(prixVenteInjection)).Map();

            //règle 3 : pointAccess obligatoire
            if (pointAccessId == Guid.Empty)
                return Result<ProfilEnergie>.Invalid(new ValidationError(
                    nameof(pointAccessId),
                    "Le point d'accès est obligatoire",
                    "ProfilEnergie.PointAccessObligatoire",
                    ValidationSeverity.Error));

            //création
            return Result.Success(new ProfilEnergie
            {
                Id = Guid.NewGuid(),
                PointAccessId = pointAccessId,
                DemandeEnergie_kWh = demande,
                OffreEnergie_kWh = offre,
                PrixAchatCible_Eur = prixAchatCible,
                PrixVenteCible_Eur = prixVenteCible,
                ConsommationAnnuelleEstime_kWh = consommationAnnuelle,
                ProductionAnnuelleEstime_kWh = productionAnnuelle,
                PrixAchatEnergieFournisseur_Eur = prixAchatFournisseur,
                PrixVenteInjectionFournisseurActuel_Eur = prixVenteInjection,
                AccordConsentement = true,
                DateAccordConsentement = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }





        //Règle de gesiton : 
        public void RetirerConsentement()  //par défaut le consentement est donnée   //Quid de la date de retrait du consentement?  
        {
            AccordConsentement = false;
            DateRetraitConsentement = DateTime.UtcNow;
        }
        public void DonnerConsentement()
        {
            AccordConsentement = true;
            DateAccordConsentement = DateTime.UtcNow;
            DateRetraitConsentement = null;
        }

        public Result VerifierEligibiliteMatching()
        {
            if (!AccordConsentement)
                return ProfilEnergieErrors.ConsentementRequis();

            var aUneOffre = OffreEnergie_kWh.HasValue && OffreEnergie_kWh.Value > 0;
            var aUneDemande = DemandeEnergie_kWh.HasValue && DemandeEnergie_kWh.Value > 0;

            if (!aUneOffre && !aUneDemande)
                return ProfilEnergieErrors.OffreOuDemandeRequise();

            return Result.Success();
        }

    }

   
}
