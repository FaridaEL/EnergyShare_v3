using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class User
    {
        [Key]    
        public Guid Id { get; set; } //Globally Unique Identifier

        public UserStatus Status { get; private set; } = UserStatus.Actif; //Statut du membre (Actif, Inactif)  ( ex après un délai d'inactivité passerait automatiquement en inactif..)
        //Obligatoire à l'inscription : uniquement mail + password pour faciliter l'inscription
        [Required, EmailAddress]
        public string Email { get; private set; } = null!;  //null indique qu'il ne faut pas envoyer d'avertissement de non-nullabilité 
        [Required]
        public string PasswordHash { get; private set; } = null!; //hash du mot de passe pour l'authentification du membre

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }

        //Si société 
        
        public string? SocieteName { get; private set; }  // Null si particulier  ou client protégé, obligatoire si professionnel

        [RegularExpression(@"^BE\d{10}$", ErrorMessage = "Format invalide (BE + 10 chiffres)")]             
        public string? NumeroEntreprise { get; private set; } // Null si particulier, commence par BE suivi de 10 chiffres

        //Données de naviguation 
            
        public Guid? OrganismePublicId { get; set; }
        [ForeignKey("OrganismePublicId")]
        public OrganismePublic? OrganismePublic { get; set; }

        //Enumérations
       
        public UserRole Role { get; private set; }   //Role : User(Acheteur, Vendeur), OrganismePublic, Administrateur.
        public UserType UserType { get; private set; }    // professionnel ou particulier ou client protégé
        public FormeLegale? FormeLegaleType { get; private set; } //Forme légale : indépendant, SPRL, asbl

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

        //Constructeur


        private User() { } // Constructeur sans parametre requis par Entity Framework Core.EF Core -->private pour empecher la creation d'un membre invalide.

        public User(
            string email,
            string passwordHash,
            UserRole role,
            UserType userType)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("L'email est obligatoire.", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Le mot de passe hashé est obligatoire.", nameof(passwordHash));

            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            Role = role;
            UserType = userType;

            ValidateUser();
        }
        //Méthodes 
        private void ValidateUser()
        {
            if (UserType == UserType.Residentiel && FormeLegaleType != null)
                throw new InvalidOperationException("Un utilisateur résidentiel ne peut pas avoir de forme légale.");

            if (UserType == UserType.Professionnel && FormeLegaleType == null)
                throw new InvalidOperationException("Un utilisateur professionnel doit avoir une forme légale.");

            if (UserType != UserType.Professionnel && !string.IsNullOrWhiteSpace(SocieteName))
                throw new InvalidOperationException("Seul un utilisateur professionnel peut avoir un nom de société.");

            if (UserType != UserType.Professionnel && !string.IsNullOrWhiteSpace(NumeroEntreprise))
                throw new InvalidOperationException("Seul un utilisateur professionnel peut avoir un numéro d’entreprise.");

            if (UserType == UserType.Professionnel && FormeLegaleType != null && FormeLegaleType != FormeLegale.PersonnePhysique)
            {
                if (string.IsNullOrWhiteSpace(SocieteName))
                    throw new InvalidOperationException("Le nom de société est obligatoire pour une personne morale.");
            }
        }


        

        public void UpdateUserIdentity(string? firstName, string? lastName, string? phoneNumber)
        {
            FirstName = firstName?.Trim();
            LastName = lastName?.Trim();
            PhoneNumber = phoneNumber?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLegalInformation(
            UserType userType,
            FormeLegale? formeLegale,
            string? societeName,
            string? numeroEntreprise)
                {
                    UserType = userType;
                    FormeLegaleType = formeLegale;
                    SocieteName = societeName?.Trim();
                    NumeroEntreprise = numeroEntreprise?.Trim();

                    ValidateUser();
                    UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Le hash du mot de passe est obligatoire.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()    //méthode pour l'administrateur pour désactiver un compte utilisateur (ex: en cas de non respect des règles de la plateforme ou d'inactivité prolongée)
        {
            Status = UserStatus.Inactif;
            UpdatedAt = DateTime.UtcNow;
        }

    }
   
}
