using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
        public bool AccordConsentement { get; set; } = true;
        public DateTime DateAccordConsentement { get; set; } = DateTime.UtcNow;

        public Guid PointAccessId { get; set; }
        [ForeignKey("PointAccessId")]
        public PointAccess PointAccess { get; set; } = null!;

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }

   
}
