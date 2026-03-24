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
        public User? GestionnairePartage { get; set; } 


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
        public int NombreParticipants => Membres.Count(m => m.ExitAt == null); // 1 Vendeur + les acheteurs

        //Méthodes : ajouter membres, document, supprimer

        public void AjouterMembre(MembrePartage membre)
        {
            if (membre is null)
                throw new ArgumentNullException(nameof(membre));

            if (Statut == PartageEnergieStatutType.EnCoursCloture)
                throw new InvalidOperationException("Impossible d'ajouter un membre à un partage en cours de clôture.");
            if (Statut == PartageEnergieStatutType.Cloture)
                throw new InvalidOperationException("Impossible d'ajouter un membre à un partage clôturé.");
            //if (membre.PartageId != Id && membre.PartageId != Guid.Empty)
            //  throw new InvalidOperationException("Le membre appartient déjà à un autre partage.");  //pas correcte il peut appartenr 

            Membres.Add(membre);
            VerifierNombreMembres();
        }

        public void AjouterDocument(DocumentPartage document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            Documents.Add(document);
        }

        public void AjouterMethodeRepartition(MethodeRepartitionInjection methode)
        {
            if (methode is null)
                throw new ArgumentNullException(nameof(methode));

            VerifierMethodeRepartition(methode);

            HistoriqueMethodes.Add(methode);
        }


        //Règles de gestion : nombre de membres  et méthode de répartition 
        public void VerifierNombreMembres()
        {
            var nbActifs = Membres.Count(m => m.ExitAt == null);

            if (EnergieType == PartageEnergieType.PairToPair && nbActifs != 2)
                throw new InvalidOperationException("Un partage pair-à-pair doit contenir exactement deux membres.");

            if (EnergieType == PartageEnergieType.MemeBatiment && nbActifs < 2)
                throw new InvalidOperationException("Un partage de type même bâtiment doit contenir au moins deux membres.");
             // Todo : les membres doivent résider à la meme addresse ! + le périmetre est d'office A --> à ajouter dans point d'accès
        
        }

        public void VerifierMethodeRepartition(MethodeRepartitionInjection? methode)
        {
            if (EnergieType == PartageEnergieType.PairToPair && methode is not null)
                throw new InvalidOperationException("Une méthode de répartition n'est pas nécessaire pour un partage pair-à-pair.");

            if (EnergieType != PartageEnergieType.PairToPair && methode is null)
                throw new InvalidOperationException("Une méthode de répartition est requise pour ce type de partage.");
        }


        //Règles de Gestion RG-E031 à RG041  : statut du partage +cf. enum PartageEnergieStatutType :

        //Pour un nouveau partage
        public void SoumettreNouveauPartageAuGrd()
        {
            if (Statut is not PartageEnergieStatutType.Inactif)
                throw new InvalidOperationException("Seul un partage inactif peut être soumis au GRD.");

            Statut = PartageEnergieStatutType.EnAttenteValidation;
        }

        public void ValiderNouveauPartageParGrd()
        {
            if (Statut is not PartageEnergieStatutType.EnAttenteValidation)
                throw new InvalidOperationException("Le partage doit être en attente de validation.");

            VerifierNombreMembres();

            Statut = PartageEnergieStatutType.Actif;
            DateDebut = DateTime.UtcNow; // La date de début est fixée à la validation par le GRD
        }

        public void RefuserNouveauParGrd()
        {
            if (Statut is not PartageEnergieStatutType.EnAttenteValidation)
                throw new InvalidOperationException("Le partage doit être en attente de validation.");

            Statut = PartageEnergieStatutType.Inactif;
        }
         //Pour les modifications
        public void DemanderModification()
        {
            if (Statut != PartageEnergieStatutType.Actif)
                throw new InvalidOperationException("Seul un partage actif peut passer en attente de modification.");

            Statut = PartageEnergieStatutType.EnAttenteModification;
        }

        public void ValiderModificationPartageParGrd()
        {
            if (Statut is not PartageEnergieStatutType.EnAttenteModification)
                throw new InvalidOperationException("Le partage doit être en attente de modification.");

            Statut = PartageEnergieStatutType.Actif;
        }

      
        public void RefuserModificationPartageParGrd()  
        {
            if (Statut != PartageEnergieStatutType.EnAttenteModification)
                throw new InvalidOperationException("Seul un partage en attente de modification peut être suspendu.");

            Statut = PartageEnergieStatutType.Suspendu;
        }

        //Fin de vie
        public void DemarrerCloture()   //Dès le début du délai de préavis de 3 semaines ou après la date de fin du partage si elle est connue et jusquà la validation de la clôture par le GRD, le partage est en cours de clôture. Pendant cette période, les participants peuvent continuer à consommer et produire de l'énergie, mais aucun nouveau participant ne peut rejoindre le partage et les membres existants ne peuvent pas augmenter leur volume de consommation ou de production.
        {
            if (Statut != PartageEnergieStatutType.Actif)
                throw new InvalidOperationException("Seul un partage actif peut entrer en cours de clôture.");

            Statut = PartageEnergieStatutType.EnCoursCloture;
        }

        public void Cloturer()
        {
            if (Statut != PartageEnergieStatutType.EnCoursCloture)
                throw new InvalidOperationException("Le partage doit être en cours de clôture.");

            Statut = PartageEnergieStatutType.Cloture;
            DateFin = DateTime.UtcNow; // La date de fin est fixée à la date de clôture effective
        }


    }
}
