using Ardalis.Result;
using EnergyShare_v3.Bricks.Model;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.Partages
{
    public class Partage  :IAuditable
    {
        //Seul l'interlocuteur unique, càd le vendeur, peut créer un partage.
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Nom { get; private set; } = null!;
        public string? Description { get; set; }
        public DateTime? DateDebut { get; private set; }
        public DateTime? DateFin { get; private set; }
      
        
        //énumération 
        //private set --> ref. entité riche --> meilleure controle de la modification : à  utiliser dès que la propriété ne doit pas
        //être modifiée librement, mais uniquement via des méthodes métier 
        public PartageEnergieStatutType Statut { get; private set; } = PartageEnergieStatutType.Inactif; // au moment de la création est en inactif  Mais comment gérer cela avec l'historique des statuts?
        public PartageEnergieType EnergieType { get; private set; }

        public PerimetreType? Perimetre { get; set; } //A pour même batiment, sinon connu après dde info aurpès de Sibelga
                                                      //Lien tables et clés étrangères


        /*le code doit être :

            - créé lorsque le créateur clique sur "Inviter membres" ;
            - unique ;
            - non devinable;
            - stocké dans Partage ;
            - utilisé ensuite pour créer une ParticipationPartage 
            - expiré au bout de 24 heures */

        [MaxLength(32)]
        public string? InvitationCode { get; private set; }
        public DateTime? InvitationCodeExpiresAt { get; private set; }

        public Guid VendeurId { get; set; } // L'ID du créateur
        [ForeignKey("VendeurId")]
        public User Vendeur { get; set; } = null!;

        public Guid? GestionnairePartageId { get; set; } // L'ID du Gestionnaire s'il y en a un, sinon null   //Est-ce que le vendeur-producteur va gérer le partage ou déleguer à un gestionnaire de partage?
        [ForeignKey("GestionnairePartageId")]
        public User? GestionnairePartage { get; set; } 


        public ICollection<ParticipationPartage> Membres { get; set; } = []; // On suppose que chaque membre ajouté à signer la convention  et/ou le fera avant la validation vers Sibelga

        public  ICollection<TarifAccord> TarifsAccord { get; set; } = []; 
        public ICollection<DataPartage> RelevesSibelga { get; set; } = [];
        public ICollection<DocumentPartage> Documents { get; set; } = [];
        public ICollection<DemandeGRD> DemandesGrd { get; set; }   = [];

        // Données d'audit
        public AuditInfo Audit { get; private set; } = new AuditInfo();

        //Données calculées
        [NotMapped]
        public int NombreParticipants => Membres.Count(m => m.ExitAt == null); // 1 Vendeur + les acheteurs

       
        
        //Constructeur
        private Partage() { } // Constructeur privé pour EF Core
        //constructeur privé métier  à utiliser avec public Static Result <Partage> Create() pour valider les règles métier avant de créer une instance de Partage
        private Partage(string nom, PartageEnergieType energieType, Guid vendeurId)
        {
            //if (string.IsNullOrWhiteSpace(nom))
              //  throw new ArgumentException("Le nom du partage ne peut pas être vide.", nameof(nom));
            Id = Guid.NewGuid();
            Nom = nom.Trim();
            EnergieType = energieType;
            VendeurId = vendeurId;
            Statut = PartageEnergieStatutType.Inactif; // Un nouveau partage commence toujours en statut Inactif
            

        }
        public static Result<Partage> Create(
            string nom,
            PartageEnergieType energieType,
            Guid vendeurId)
                {
                    if (string.IsNullOrWhiteSpace(nom))
                        return PartageErrors.NomObligatoire().Map();
                    if (vendeurId == Guid.Empty)
                        return PartageErrors.VendeurObligatoire().Map();

                    return Result.Success(new Partage(
                        nom,
                        energieType,
                        vendeurId));
        }

        //Méthodes : ajouter membres, document, supprimer

        public Result AjouterMembre(ParticipationPartage membre)
        {
            ArgumentNullException.ThrowIfNull(membre);
            //if (membre is null)
              //  throw new ArgumentNullException(nameof(membre));

            if (Statut == PartageEnergieStatutType.EnCoursCloture)
              return PartageErrors.PartageEnCoursDeCloture();
            if (Statut == PartageEnergieStatutType.Cloture)
              return PartageErrors.PartageCloture();
            //if (membre.PartageId != Id && membre.PartageId != Guid.Empty)
            //  throw new InvalidOperationException("Le membre appartient déjà à un autre partage.");  //pas correcte il peut appartenr 

            Membres.Add(membre);

            // IMPORTANT :
            // On ne vérifie pas ici le nombre minimum/maximum de membres, sinon un partage P-to-P
            // refuserait le premier membre ajouté, car il attend exactement 2 membres.
            // La validation du nombre de membres se fera donc au moment de la soumission au GRD lorsqu'on considère que la 
            // composition du partage est complète.

            //var validation = VerifierNombreMembres();
            //if (!validation.IsSuccess)
            //{
            //    Membres.Remove(membre);
            //    return validation;
            //}
            Audit.Touch(null);
            return Result.Success();
        }

        public Result AjouterDocument(DocumentPartage document)
        {
            ArgumentNullException.ThrowIfNull(document);
           // if (document is null)
             //   throw new ArgumentNullException(nameof(document));
            if (Statut == PartageEnergieStatutType.EnCoursCloture)
                return PartageErrors.PartageEnCoursDeCloture();

            if (Statut == PartageEnergieStatutType.Cloture)
                return PartageErrors.PartageCloture();

            Documents.Add(document);
            Audit.Touch(null);
            return Result.Success();
        }


        //Règles de gestion : nombre de membres  et méthode de répartition 
        public Result VerifierNombreMembres()
        {
            var nbActifs = Membres.Count(m => m.ExitAt == null);

            if (EnergieType == PartageEnergieType.PairToPair && nbActifs != 2)
                return PartageErrors.NombreMembresPairToPairInvalide();;

            if (EnergieType == PartageEnergieType.MemeBatiment && nbActifs < 2)
                return PartageErrors.NombreMembresMemeBatimentInvalide();
             // Todo : les membres doivent résider à la meme addresse ! + le périmetre est d'office A --> à ajouter dans point d'accès
        
            return Result.Success();
        }

        public Result Renommer(string nouveauNom)
        {
            if (Statut == PartageEnergieStatutType.EnCoursCloture)
                return PartageErrors.PartageEnCoursDeCloture();

            if (Statut == PartageEnergieStatutType.Cloture)
                  return PartageErrors.PartageCloture();

            if (string.IsNullOrWhiteSpace(nouveauNom))
                 return PartageErrors.NomObligatoire();

            Nom = nouveauNom.Trim();
            Audit.Touch(null);
            return Result.Success();
        }

        //Règles de Gestion RG-E031 à RG041  : statut du partage +cf. enum PartageEnergieStatutType :

        //Pour un nouveau partage
        public Result SoumettreNouveauPartageAuGrd()
        {
            if (Statut is not PartageEnergieStatutType.Inactif)
                return PartageErrors.SoumissionGrdImpossible();


            var validationNombreMembres = VerifierNombreMembres();
            if (!validationNombreMembres.IsSuccess)
                return validationNombreMembres;

            Statut = PartageEnergieStatutType.EnAttenteValidation;
            Audit.Touch(null);
            return Result.Success();
        }

        public Result ValiderNouveauPartageParGrd()
        {
            if (Statut is not PartageEnergieStatutType.EnAttenteValidation)
                return PartageErrors.ValidationGrdImpossible();

            var validation = VerifierNombreMembres();
            if (!validation.IsSuccess)
                return validation;


            Statut = PartageEnergieStatutType.Actif;
            DateDebut = DateTime.UtcNow; // La date de début est fixée à la validation par le GRD
            Audit.Touch(null);
            return Result.Success();
        }

        public Result RefuserNouveauParGrd()
        {
            if (Statut is not PartageEnergieStatutType.EnAttenteValidation)
                return PartageErrors.ValidationGrdImpossible();

            Statut = PartageEnergieStatutType.Inactif;
            Audit.Touch(null);
            return   Result.Success();
        }
         //Pour les modifications
        public Result DemanderModification()
        {
            if (Statut != PartageEnergieStatutType.Actif)
                 return PartageErrors.ModificationImpossible();

            Statut = PartageEnergieStatutType.EnAttenteModification;
            Audit.Touch(null);
            return Result.Success();
        }

        public Result ValiderModificationPartageParGrd()
        {
            if (Statut is not PartageEnergieStatutType.EnAttenteModification)
                return PartageErrors.ValidationModificationGrdImpossible();

            Statut = PartageEnergieStatutType.Actif;
            Audit.Touch(null);
            return Result.Success();
        }

        public Result RefuserModificationPartageParGrd()  
        {
            if (Statut != PartageEnergieStatutType.EnAttenteModification)
                return PartageErrors.ValidationModificationGrdImpossible();

            Statut = PartageEnergieStatutType.Suspendu;
            Audit.Touch(null);
            return Result.Success();
        }

        //Fin de vie
        public Result DemarrerCloture()   //Dès le début du délai de préavis de 3 semaines ou après la date de fin du partage si elle est connue et jusquà la validation de la clôture par le GRD, le partage est en cours de clôture. Pendant cette période, les participants peuvent continuer à consommer et produire de l'énergie, mais aucun nouveau participant ne peut rejoindre le partage et les membres existants ne peuvent pas augmenter leur volume de consommation ou de production.
        {
            if (Statut != PartageEnergieStatutType.Actif)
                return PartageErrors.DemarrageClotureImpossible();

            Statut = PartageEnergieStatutType.EnCoursCloture;
            Audit.Touch(null);
            return Result.Success();
        }

        public Result Cloturer()
        {
            if (Statut != PartageEnergieStatutType.EnCoursCloture)
                return PartageErrors.ClotureImpossible();

            Statut = PartageEnergieStatutType.Cloture;
            DateFin = DateTime.UtcNow; // La date de fin est fixée à la date de clôture effective
            Audit.Touch(null);
            return Result.Success();
        }


        //Mettre à jour les données du partage : nom, description, dates de début et de fin (tant que le partage n'est pas soumis au GRD), ajouter des membres ou des documents (tant que le partage n'est pas en cours de clôture ou clôturé).
        public Result Update(
            string nom,
            string? description,
            PartageEnergieType energieType,
            DateTime? dateDebut,
            DateTime? dateFin)
            {
                if (string.IsNullOrWhiteSpace(nom))
                    return PartageErrors.NomObligatoire();

                if (dateDebut.HasValue && dateFin.HasValue && dateFin.Value < dateDebut.Value)
                    return PartageErrors.DateFinAvantDateDebut();

                Nom = nom.Trim();
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
                EnergieType = energieType;
                DateDebut = dateDebut;
                DateFin = dateFin;

                Audit.Touch(null);

                return Result.Success();
        }

        private static string GenerateInvitationCode()
        {    //Guid : génere un identifiant unique
            //Replace : nettoyer les caractères spéciaux pour obtenir une chaîne alphanumérique
            // .ToUpperInvariant() : uniformise le code en majuscules
            
            return Guid.NewGuid()
                .ToString()
                .Replace("-", "")
                .ToUpperInvariant()[..12];

        }

        public  Result EnsureValidInvitationCode()
        {
            if (Statut == PartageEnergieStatutType.EnCoursCloture)
                return PartageErrors.PartageEnCoursDeCloture();

            if (Statut == PartageEnergieStatutType.Cloture)
                return PartageErrors.PartageCloture();

            if (string.IsNullOrWhiteSpace(InvitationCode) ||
                InvitationCodeExpiresAt is null ||
                InvitationCodeExpiresAt <= DateTime.UtcNow)
            {
                InvitationCode = GenerateInvitationCode();
                InvitationCodeExpiresAt = DateTime.UtcNow.AddHours(24);
                Audit.Touch(null);
            }

            return Result.Success();
        }
        //FAire une dde info périmetre  
        public Result AjouterDemandeGrd(DemandeGRD demande)
        {
            if (demande == null)
                throw new ArgumentNullException(nameof(demande));

            if (Statut == PartageEnergieStatutType.EnCoursCloture ||
                Statut == PartageEnergieStatutType.Cloture)
            {
                return PartageErrors.DemandePerimetreImpossible();
            }

            DemandesGrd.Add(demande);
            Audit.Touch(null);

            return Result.Success();
        }
        public Result DefinirPerimetre(PerimetreType perimetre)
        {
            if (Statut == PartageEnergieStatutType.Cloture ||
                Statut == PartageEnergieStatutType.EnCoursCloture)
                return PartageErrors.DemandePerimetreImpossible();

            Perimetre = perimetre;
            Audit.Touch(null);

            return Result.Success();
        }


    }
}
