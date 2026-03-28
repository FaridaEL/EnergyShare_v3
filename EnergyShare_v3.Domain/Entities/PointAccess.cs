using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{        /*Un code EAN n’est pas forcément unique dans toute l’histoire de l’application, 
          * car un même point d’accès peut changer de titulaire au cours du temps ( déménagement, reprise de contrat).
          * En revanche, un même EAN / point d’accès actif ne peut être rattaché qu’à un seul utilisateur actif à la fois,
          * ni participer à plusieurs partages actifs simultanément.*/
    public class PointAccess
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
        //permet de déterminer si le point injecte sur le résau et donc est un producteur/vendeur
        //seul un point d'injection peut être désigné comme interlocuteur unique du partage et vendre de l'énergie
        public bool IsInjectionPoint { get; set; } = false;  
        
        //règle métier : compteur intelligent obligatoire, commence par 1SJ et longueur maximale 20 caractères
        
        [MaxLength(20)]
        [RegularExpression(@"^1SJ.{0,17}$")]
        public string? SmartMeter_Encrypted { get; set; } //numéro de compteur intelligent chiffré pour garantir la confidentialité des données de consommation/production d'énergie
        
        //règle métier : commence par 5414489 et comporte 18 chiffres
        
        [RegularExpression(@"^5414489\d{11}$")]
        public string? EAN_Encrypted { get; set; } //numéro EAN chiffré pour garantir la confidentialité des données de consommation/production d'énergie
        

        public ICollection<MembrePartage> Membres { get; set; } = [];

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;  //règle métier : point d'accès rattaché à un utilisateur actif

        [Required]    //règle métier : point d'accès couvert par un contrat d'énergie
        public Guid FournisseurId { get; set; }
        [ForeignKey("FournisseurId")]
        public FournisseurEnergie Fournisseur { get; set; } = null!;

        //enumérations
        public SourceRenouvelable? Source { get; set; }
        //Naviguation 
        
        public ProfilEnergie? ProfilEnergie { get; set; }
        public ICollection<Match> MatchsAcheteurs { get; set; } = [];
        public ICollection<Match> MatchsVendeurs { get; set; } = [];

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
              
        //Methodes

        public bool estCompletPourPartage()
        {
            //Un point d'accès est considéré comme complet pour le partage
            //s'il dispose d'une adresse complète (AdresseLine1 et CodePostal),
            //d'un numéro de compteur intelligent chiffré, d'un numéro EAN chiffré,
            //et s'il est rattaché à un utilisateur actif et à un fournisseur d'énergie.
            return !string.IsNullOrEmpty(AdresseLine1) 
                && !string.IsNullOrEmpty(CodePostal) 
                && !string.IsNullOrEmpty(SmartMeter_Encrypted) 
                && !string.IsNullOrEmpty(EAN_Encrypted) 
                && User != null && User.Status == UserStatus.Actif
                && Fournisseur != null;
        }

    }

   
}
