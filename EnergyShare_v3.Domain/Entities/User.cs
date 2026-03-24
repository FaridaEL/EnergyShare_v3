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

        public UserStatus Status { get; set; } = UserStatus.Actif; //Statut du membre (Actif, Inactif)  ( ex après un délai d'inactivité passerait automatiquement en inactif..)
        //Obligatoire à l'inscription : uniquement mail + password pour faciliter l'inscription
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
        public string? SocieteName { get; set; }  // Null si particulier
        
        [RegularExpression(@"^BE\d{10}$", ErrorMessage = "Format invalide (BE + 10 chiffres)")]             
        public string? NumeroEntreprise { get; set; } // Null si particulier, commence par BE suivi de 10 chiffres

        //Données de naviguation 
            
        public Guid? OrganismePublicId { get; set; }
        [ForeignKey("OrganismePublicId")]
        public OrganismePublic? OrganismePublic { get; set; }

        //Enumérations
       
        public UserRole Role { get; set; }   //Role : Acheteur, Vendeur, OrganismePublic, Administrateur.
        public UserType UserType { get; set; }    // professionnel ou particulier
        public SocieteType? SocieteType { get; set; }

        //naviguation
        public ICollection<Message> MessagesEnvoyes { get; set; } = [];
        public ICollection<Message> MessagesRecus { get; set; } = [];

        public ICollection<PointAccess> PointsAccess { get; set; } = [];   //1 user peut avoir plusieurs points d'accès Mais un même point d'accès ne peut être rattaché qu'à un seul partage atif à la fois

        public ICollection<MembrePartage> MembresPartage { get; set; } = [];
        public ICollection<Partage> PartagesCrees { get; set; } = [];
        public ICollection<Partage> PartagesGeres { get; set; } = [];

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
               

        /// <summary>Nom complet (propriete calculee, logique metier dans le domaine)</summary>
        
        public string? FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
                                  ? Email
                                  : $"{FirstName} {LastName}".Trim();


    }
   
}
