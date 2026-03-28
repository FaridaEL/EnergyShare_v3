using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class ProfilFacturation
    {
        /* utilisé pour la création de facture*/
        [Key]    
        public Guid Id { get; set; }

        // énumérations
        public SituationFiscale SituationFiscale { get; set; }

        public string TitulaireCompte { get; set; } = null!; // Nom du titulaire du compte, utilisé pour la facturation et les documents contractuels
        public string? IBAN { get; set; }

        public string? BIC { get; set; }
        public string? NumeroCompteBancaire { get; set; }
        public string? NumeroTVA { get; set; }
        public string? AdresseFacturation { get; set; }


        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }

   
}
