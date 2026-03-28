using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
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
        public ProfilEnergie(
            decimal? demandeEnergie_kWh,
        decimal? offreEnergie_kWh,
        decimal? prixAchatCible_Eur,
        decimal? prixVenteCible_Eur,
        decimal? consommationAnnuelleEstime_kWh,
        decimal? productionAnnuelleEstime_kWh,
        decimal? prixAchatEnergieFournisseur_Eur,
        decimal? prixVenteInjectionFournisseurActuel_Eur,
        Guid pointAccessId) {
            if (!demandeEnergie_kWh.HasValue && !offreEnergie_kWh.HasValue)
                throw new ArgumentException("Le profil doit contenir une demande ou une offre.");

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
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;


        } //constructeur pour la création d'un profil énergie, le consentement est donné par défaut



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

        public void VerifierEligibiliteMatching()
        {
            if (!AccordConsentement)
                throw new InvalidOperationException("Le consentement au partage des données énergétiques est requis.");

            var aUneOffre = OffreEnergie_kWh.HasValue && OffreEnergie_kWh.Value > 0;
            var aUneDemande = DemandeEnergie_kWh.HasValue && DemandeEnergie_kWh.Value > 0;

            if (!aUneOffre && !aUneDemande)
                throw new InvalidOperationException("Le profil énergétique doit contenir une offre ou une demande d'énergie.");
        }

    }

   
}
