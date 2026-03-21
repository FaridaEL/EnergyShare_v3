using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
        public string? numeroCompteBancaire { get; set; }
        public string? numéroTVA { get; set; }
        public string? adresseFacturation { get; set; }


        public Guid? UserID { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }

   
}
