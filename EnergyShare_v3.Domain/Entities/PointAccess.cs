using Ardalis.Result;
using EnergyShare_v3.Bricks.Model;
using EnergyShare_v3.Domain.Entities.Matchs;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{        /*Un code EAN n’est pas forcément unique dans toute l’histoire de l’application, 
          * car un même point d’accès peut changer de titulaire au cours du temps ( déménagement, reprise de contrat).
          * En revanche, un même EAN / point d’accès actif ne peut être rattaché qu’à un seul utilisateur actif à la fois,
          * ni participer à plusieurs partages actifs simultanément.*/
    public class PointAccess  :IAuditable
    {
        [Key]    
        public Guid Id { get; set; } 
        public string? AdresseLine1  { get; set; } 
        //règle métier : partage situé en Région bruxelloise
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Le code postal doit contenir 4 chiffres")] 
        public string? CodePostal { get; set; }
        /* TODO : calcule distances entre membres : Il faut créer un service pour convertir l'adresse en données géolocalisable 
         * --> public async Task<(double lat, double lng)> GetCoordinates(string AdresseLine1, string codePostal)*/
        public double? Latitude { get; set; }  
        public double? Longitude { get; set; }
        
        public bool IsInjectionPoint { get; set; } = false; //permet de déterminer si le point injecte sur le résau et donc est un producteur/vendeur  -->    //seul un point d'injection peut être désigné comme interlocuteur unique du partage et vendre de l'énergie

        [Required]    //règle métier : point d'accès couvert par un contrat d'énergie
        public string Fournisseur { get; set; } =null!; //liste définie en UI

        [MaxLength(20)]
        [RegularExpression(@"^1SJ.{0,17}$")]  //règle métier : compteur intelligent obligatoire, commence par 1SJ et longueur maximale 20 caractères
        public string? SmartMeter_Encrypted { get; set; } //numéro de compteur intelligent chiffré pour garantir la confidentialité des données de consommation/production d'énergie
        
        [RegularExpression(@"^5414489\d{11}$")]   //règle métier : commence par 5414489 et comporte 18 chiffres
        public string? EAN_Encrypted { get; set; } //numéro EAN chiffré pour garantir la confidentialité des données de consommation/production d'énergie
   
        public ICollection<ParticipationPartage> Membres { get; set; } = [];

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;  //règle métier : point d'accès rattaché à un utilisateur actif

        [Required] //rgèle métier :  l'utlisateur doit donner son consentement pour le partage de ses données.
        public bool AccordConsentement { get; private set; } = true;
        public DateTime DateAccordConsentement { get; private set; } = DateTime.UtcNow;
        public DateTime? DateRetraitConsentement { get; private set; }

        //Naviguation 
        public ProfilEnergie? ProfilEnergie { get; set; }
        public ICollection<Match> MatchsAcheteurs { get; set; } = [];
        public ICollection<Match> MatchsVendeurs { get; set; } = [];

        //Données d'audit
        public AuditInfo Audit { get; private set; } = new AuditInfo();

        //Methodes
        public bool EstCompletPourPartage()
        {
            //Un point d'accès est considéré comme complet pour le partage si il dispoe :
            //d'une adresse complète (AdresseLine1 et CodePostal),
            //d'un numéro de compteur intelligent et un numéro EAN, de préférence chiffré,
            //et s'il est rattaché à un utilisateur actif et à un fournisseur d'énergie.
            return !string.IsNullOrEmpty(AdresseLine1)
                && !string.IsNullOrEmpty(CodePostal)
                && !string.IsNullOrEmpty(SmartMeter_Encrypted)
                && !string.IsNullOrEmpty(EAN_Encrypted)
                &&!string.IsNullOrWhiteSpace(Fournisseur)
                && User != null && User.Status == UserStatus.Actif
                && AccordConsentement;
        }

       /* public Result VerifierEligibilitePartage()
        {
            if (!AccordConsentement)
                return ProfilEnergieErrors.ConsentementRequis();

            var aUneOffre = OffreEnergie_kWh.HasValue && OffreEnergie_kWh.Value > 0;
            var aUneDemande = DemandeEnergie_kWh.HasValue && DemandeEnergie_kWh.Value > 0;

            if (!aUneOffre && !aUneDemande)
                return ProfilEnergieErrors.OffreOuDemandeRequise();

            return Result.Success();
        }*/

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

    }

   
}
