using Ardalis.Result;
using EnergyShare_v3.Bricks.Model;
using EnergyShare_v3.Domain.Entities.Messages;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EnergyShare_v3.Domain.Entities.Users
{
    public class User : IdentityUser<Guid>,  IAuditable
    {
        //[Key]    
        //public Guid Id { get; set; } //Globally Unique Identifier

        public UserStatus Status { get; private set; } = UserStatus.Actif; //Statut du membre (Actif, Inactif)  ( ex après un délai d'inactivité passerait automatiquement en inactif..)
                                                                           //Obligatoire à l'inscription : uniquement mail + password pour faciliter l'inscription
        public UserRole Role { get; private set; }   //Role : User(Acheteur, Vendeur), OrganismePublic, Administrateur.
        //[Required]
        //public Email Email { get; private set; } = null!;  //null indique qu'il ne faut pas envoyer d'avertissement de non-nullabilité 
        //[Required]
        //public string PasswordHash { get; private set; } = null!; //hash du mot de passe pour l'authentification du membre

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        //[Phone]
        //public string? PhoneNumber { get; set; }

        //Si société 
        public string? SocieteName { get; private set; }  // Null si particulier  ou client protégé, obligatoire si professionnel

        [RegularExpression(@"^BE\d{10}$", ErrorMessage = "Format invalide (BE + 10 chiffres)")]             
        public string? NumeroEntreprise { get; private set; } // Null si particulier, commence par BE suivi de 10 chiffres

        //Données de naviguation 
            
        public Guid? OrganismePublicId { get; set; }
        [ForeignKey("OrganismePublicId")]
        public OrganismePublic? OrganismePublic { get; set; }
        public ICollection<Message> MessagesEnvoyes { get; set; } = [];
        public ICollection<Message> MessagesRecus { get; set; } = [];
        public ICollection<PointAccess> PointsAccess { get; set; } = [];   //1 user peut avoir plusieurs points d'accès Mais un même point d'accès ne peut être rattaché qu'à un seul partage atif à la fois
        public ICollection<Partage> PartagesCrees { get; set; } = [];
        public ICollection<Partage> PartagesGeres { get; set; } = [];

        //Données d'audit
        public AuditInfo Audit { get; private set; } = new AuditInfo();

        
        [NotMapped]   //Nom complet (propriete calculee, logique metier dans le domaine)
        public string? FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
                                  ? Email
                                  : $"{FirstName} {LastName}".Trim();

        [NotMapped]
        public bool IsSociety =>
                !string.IsNullOrWhiteSpace(SocieteName) ||
                !string.IsNullOrWhiteSpace(NumeroEntreprise);

        //Constructeur

        private User() { } // Constructeur sans parametre requis par Entity Framework Core.EF Core -->private pour empecher la creation d'un membre invalide.

        private User(
            string email,
            UserRole role
            )
                {
                    UserName = email;
                    Email = email;
                    Role = role;
                    Status = UserStatus.Actif;
                    Audit.Touch(null);
        }

        //Méthodes 
        private Result ValidateUser()
        {
            if (string.IsNullOrWhiteSpace(SocieteName) && !string.IsNullOrWhiteSpace(NumeroEntreprise))
                return UserErrors.NomSocieteRequisSiNumeroEntreprise();

            return Result.Success();
        }



        public static Result<User> Create(string email, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(email))
                return UserErrors.EmailObligatoire().Map<User>();

            var user = new User(email.Trim(), role);

            var validation = user.ValidateUser();
            if (!validation.IsSuccess)
                return Result<User>.Invalid(validation.ValidationErrors);

            return Result.Success(user);
        }


        /* public static Result<User> Create(
                string email,
                string passwordHash,
                UserRole role)
            {
                var emailResult = Email.Create(email);
                 if (!emailResult.IsSuccess)
                     return Result<User>.Invalid(emailResult.ValidationErrors);
            //if (string.IsNullOrWhiteSpace(email))
            //  return UserErrors.EmailObligatoire().Map();

                 if (string.IsNullOrWhiteSpace(passwordHash))
                    return UserErrors.PasswordHashObligatoire().Map();

                var user = new User(emailResult.Value, passwordHash, role);

                var validation = user.ValidateUser();
                if (!validation.IsSuccess)
                    return Result<User>.Invalid(validation.ValidationErrors);

                return Result.Success(user);
            }   */


        public void UpdateUserIdentity(string? firstName, string? lastName, string? phoneNumber)
        {
            FirstName = firstName?.Trim();
            LastName = lastName?.Trim();
            PhoneNumber = phoneNumber?.Trim();
            Audit.Touch(null);
        }

        public Result UpdateLegalInformation(
            string? societeName,
            string? numeroEntreprise)
                {
                    SocieteName = societeName?.Trim();
                    NumeroEntreprise = numeroEntreprise?.Trim();

                    var validation = ValidateUser();
                    if (!validation.IsSuccess)
                        return validation;

                    Audit.Touch(null);
            return Result.Success();
        }

        /* Mot de passe est géré par Identity Framework, donc pas besoin de méthode spécifique dans l'entité User pour changer le mot de passe.
         * public Result ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                return UserErrors.PasswordHashObligatoire();

            PasswordHash = newPasswordHash;
            Audit.Touch(null);

            return Result.Success();
        }*/

        public void Deactivate()    //méthode pour l'administrateur pour désactiver un compte utilisateur (ex: en cas de non respect des règles de la plateforme ou d'inactivité prolongée)
        {
            Status = UserStatus.Inactif;
            Audit.Touch(null);
        }

    }
   
}
