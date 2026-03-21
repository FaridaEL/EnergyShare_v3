using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EnergyShare_v3.Domain.Entities
{
    public class User
    {
        [Key]    
        public Guid Id { get; set; } //Globally Unique Identifier

        public UserStatus Status { get; set; } = UserStatus.Actif; //Statut du membre (Actif, Inactif)
        /// <summary>---Obligatoire à l'inscription : uniquement mail + password pour faciliter l'inscription--- </summary>
        [Required, EmailAddress]
        public string Email { get; set; } = null!;  //null indique qu'il ne faut pas envoyer d'avertissement de non-nullabilité 
        [Required]
        public string PasswordHash { get; set; } = null!; //hash du mot de passe pour l'authentification du membre

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }

        //Si société 
        public bool IsSociety { get; set; }
        public string? SocieteNom { get; set; }  // Null si particulier
        
        [RegularExpression(@"^BE\d{10}$", ErrorMessage = "Format invalide (BE + 10 chiffres)")]             
        public string? NumeroEntreprise { get; set; } // Null si particulier, commence par BE suivi de 10 chiffres

        //Données de naviguation 
        public Guid? PointAcessId {  get; set; }
        [ForeignKey("PointAcessId")]
        public PointAccess? PointAccess { get; set; }

        public Guid? ProfileEnergieId { get; set; }
        [ForeignKey("ProfileEnergieId")]
        public ProfilEnergie? ProfilEnergie { get; set; }
        public Guid? PartageEnergieId { get; set; }
        [ForeignKey("PartageEnergieId")]
        public Partage? PartageEnergie { get; set; }
        public Guid? OrgansimePublicId { get; set; }
        [ForeignKey("OrgansimePublicId")]
        public OrganismePublic? OrgansimePublic { get; set; }

        //Enumérations
        public UserStatus? UserStatut { get; set; }  //Actif , inactif ( ex après un délai d'inactivité passerait automatiquement en inactif..)
        public UserRole? UserRole { get; set; }   //Role : Acheteur, Vendeur, OrganismePublic, Administrateur.
        public UserType? UserType { get; set; }    // professionnel ou particulier
        public SocieteType? SocieteType { get; set; }

        public ICollection<Match>? MatchsAcheteurs { get; set; } = new List<Match>();
        public ICollection<Match>? MatchsVendeurs { get; set; } = new List<Match>();

        public ICollection<Message>? MessageExpedieur { get; set; } = new List<Message>();
        public ICollection<Message>? MessageDestinataire { get; set; } = new List<Message>();

        public ICollection<PointAccess>? PointsAccess { get; set; } = new List<PointAccess>();   //1 user peut avoir plusieurs points d'accès Mais un même point d'accès ne peut être rattaché qu'à un seul partage atif à la fois


        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
               

        /// <summary>Nom complet (propriete calculee, logique metier dans le domaine)</summary>
        
        public string? FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
                                  ? Email
                                  : $"{FirstName} {LastName}".Trim();





    }

   
}
