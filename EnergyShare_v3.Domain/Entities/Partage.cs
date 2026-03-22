using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Pipes;

namespace EnergyShare_v3.Domain.Entities
{
    public class Partage
    {
        //Seul l'interlocuteur unique, càd le vendeur, peut créer un partage.
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Nom { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool RecevoirDataParticipant { get; set; } = false;   //permet de demander des fichiers détaillés par participant --> Message à ajouter sur l'interface: "Chaque mois, Sibelga vous enverra un fichier contenant les données vous permettant de connaitre le volume local mensuel (la consommation qui vient du partage) de chaque participant et le montant des tarifs réseau associés. Si vous le souhaitez, vous pouvez également recevoir un fichier contenant les données de chaque participant sous forme quart horaire(= par quart d’heure) en cochant la cases ci-dessous.

        //énumération 
        public PartageEnergieStatutType Statut { get; set; } = PartageEnergieStatutType.Inactif; // au moment de la création est en inactif  Mais comment gérer cela avec l'historique des statuts?
        public PartageEnergieType EnergieType { get; set; }
        public DataTransmissionType DataTransmissionType { get; set; }   //SFTP ou SharepointLink , pour envoi de données de consommation du partage et le montant des tarifs réseau associés chaque mois

        //Lien tables et clés étrangères
        public Guid? PerimetreId { get; set; } //connu qu'après dde d'infos auprès de Sibelga --> null au moment de la création du partage  ou A si même batiment
        [ForeignKey("PerimetreId")]
        public PerimetrePartageReglementaire? Perimetre { get; set; }
        public Guid VendeurId { get; set; } // L'ID du créateur
        [ForeignKey("VendeurId")]
        public User Vendeur { get; set; } = null!;

        public Guid? GestionnairePartageId { get; set; } // L'ID du Gestionnaire s'il y en a un, sinon null   //Est-ce que le vendeur-producteur va gérer le partage ou déleguer à un gestionnaire de partage?
        [ForeignKey("GestionnairePartageId")]
        public User? GestionnairePartage { get; set; } = null!;


        public ICollection<MembrePartage> Membres { get; set; } = []; // On suppose que chaque membre ajouté à signer la convention  et/ou le fera avant la validation vers Sibelga

        public  ICollection<TarifAccord> TarifsAccord { get; set; } = [];

        public ICollection<FraisComptageMesurage> HistoriqueFraisComptage { get; set; } = [];

        public ICollection<MethodeRepartitionInjection> HistoriqueMethodes { get; set; } = [];
        public ICollection<HistoriquePartageStatut> HistoriqueStatut { get; set; } = [];
        public ICollection<DataPartage> RelevesSibelga { get; set; } = [];
        public ICollection<DocumentPartage> Documents { get; set; } = [];
        public ICollection<DdeValidationPartage> Validations { get; set; }   = [];
        public ICollection<DdeInfoPerimetre> DemandesInfos { get; set; }    = [];

        

        // Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //Données calculées
        [NotMapped]
        public int NombreParticipants => Membres.Count; // 1 Vendeur + les acheteurs
    }
}
