using Ardalis.Result;
using EnergyShare_v3.Bricks.Model;
using EnergyShare_v3.Domain.Entities.Messages;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EnergyShare_v3.Domain.Entities.Users
{
    public class User : IAuditable
    {
        [Key]    
        public Guid Id { get; set; } //Globally Unique Identifier

        public UserStatus Status { get; private set; } = UserStatus.Actif; //Statut du membre (Actif, Inactif)  ( ex après un délai d'inactivité passerait automatiquement en inactif..)
        //Obligatoire à l'inscription : uniquement mail + password pour faciliter l'inscription
        [Required]
        public Email Email { get; private set; } = null!;  //null indique qu'il ne faut pas envoyer d'avertissement de non-nullabilité 
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
        // Infos de traçabilité de l'entité.
        public AuditInfo Audit { get; private set; } = new AuditInfo();
        
        /// <summary>Nom complet (propriete calculee, logique metier dans le domaine)</summary>

        public string? FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
                                  ? Email
                                  : $"{FirstName} {LastName}".Trim();

        

        //Constructeur

        private User() { } // Constructeur sans parametre requis par Entity Framework Core.EF Core -->private pour empecher la creation d'un membre invalide.

        private User(
            Email email,
            string passwordHash,
            UserRole role,
            UserType userType)
                {
                    Id = Guid.NewGuid();
                    Email = email;
                    PasswordHash = passwordHash;
                    Role = role;
                    UserType = userType;
                    Status = UserStatus.Actif;
                    Audit.Touch(null);
        }


        //Méthodes 
        private Result ValidateUser()
        {
            if (UserType == UserType.Residentiel && FormeLegaleType != null)
                return UserErrors.FormeLegaleInterditePourResidentiel();

            if (UserType == UserType.Professionnel && FormeLegaleType == null)
                return UserErrors.FormeLegaleObligatoirePourProfessionnel();

            if (UserType != UserType.Professionnel && !string.IsNullOrWhiteSpace(SocieteName))
                return UserErrors.SocieteReserveeAuProfessionnel();

            if (UserType != UserType.Professionnel && !string.IsNullOrWhiteSpace(NumeroEntreprise))
                return UserErrors.NumeroEntrepriseReserveAuProfessionnel();

            if (UserType == UserType.Professionnel &&
                FormeLegaleType != null &&
                FormeLegaleType != FormeLegale.PersonnePhysique &&
                string.IsNullOrWhiteSpace(SocieteName))
                return UserErrors.NomSocieteObligatoirePourPersonneMorale();

            return Result.Success();
        }

         public static Result<User> Create(
                string email,
                string passwordHash,
                UserRole role,
                UserType userType)
            {
                var emailResult = Email.Create(email);
                 if (!emailResult.IsSuccess)
                     return Result<User>.Invalid(emailResult.ValidationErrors);
            //if (string.IsNullOrWhiteSpace(email))
            //  return UserErrors.EmailObligatoire().Map();

                 if (string.IsNullOrWhiteSpace(passwordHash))
                    return UserErrors.PasswordHashObligatoire().Map();

                var user = new User(emailResult.Value, passwordHash, role, userType);

                var validation = user.ValidateUser();
                if (!validation.IsSuccess)
                    return Result<User>.Invalid(validation.ValidationErrors);

                return Result.Success(user);
            }


        public void UpdateUserIdentity(string? firstName, string? lastName, string? phoneNumber)
        {
            FirstName = firstName?.Trim();
            LastName = lastName?.Trim();
            PhoneNumber = phoneNumber?.Trim();
            Audit.Touch(null);
        }

        public Result UpdateLegalInformation(
            UserType userType,
            FormeLegale? formeLegale,
            string? societeName,
            string? numeroEntreprise)
                {
                    UserType = userType;
                    FormeLegaleType = formeLegale;
                    SocieteName = societeName?.Trim();
                    NumeroEntreprise = numeroEntreprise?.Trim();

                    var validation = ValidateUser();
                    if (!validation.IsSuccess)
                        return validation;

                    Audit.Touch(null);
            return Result.Success();
        }

        public Result ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                return UserErrors.PasswordHashObligatoire();

            PasswordHash = newPasswordHash;
            Audit.Touch(null);

            return Result.Success();
        }

        public void Deactivate()    //méthode pour l'administrateur pour désactiver un compte utilisateur (ex: en cas de non respect des règles de la plateforme ou d'inactivité prolongée)
        {
            Status = UserStatus.Inactif;
            Audit.Touch(null);
        }

    }
   
}
