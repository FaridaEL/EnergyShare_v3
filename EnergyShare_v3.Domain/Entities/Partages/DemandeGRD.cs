using Ardalis.Result;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.Partages
{
    public class DemandeGRD
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime DateDemande { get; set; } = DateTime.UtcNow;
        public DateTime? DateReponse { get; set; }   //  RÉPONSE DE SIBELGA 
        public string? DetailsDemande { get; set; }  // Informations complémentaires fournies par le demandeur, ex. adresses des membres ou précisions sur la demande.
        public string? CommentaireReponseGRD { get; set; }  // Champ libre pour que l'agent Sibelga puisse répondre ; ex : modifications attendues, périmetre précision,  ou tout commentaire.  
                

        // StatutDemande : EnAttente, Valide, Refus
        // Gestion statut partage :
        // si type = nouvelleActivation valide --> statusPartage : "EnAttenteValidation" , si valide --> actif, si refus --> inactif
        // si type = ModificationPartageExistant --> StatusPartage devient EnAttenteModification, si valide --> actif, siRefus --> suspendu
        //Si type =   clôturePartage --> StatusPartage devient EnAttenteCloture, si valide --> clôturé, si refus --> actif

        //Enumérations
        public DdeGRDResponseStatus ResponseStatus { get; set; } = DdeGRDResponseStatus.EnAttente;
        public DemandeGRDType DemandeType { get; set; } // NouvelleActivation, ModificationPartageExistant, clôturePartage, DdeInfos etc.

        public PerimetreType? PerimetreConfirme { get; set; } /// Si dde infos : Réponse attendue : A, B, C ou D. 
        

        //FK
        public Guid DemandeurId { get; set; }   // Lien vers l'initiateur (le Vendeur) de la demande
        [ForeignKey("DemandeurId")]
        public User Demandeur { get; set; } = null!;
        public Guid? OrganismePublicId { get; set; }
        [ForeignKey("OrganismePublicId")]
        public OrganismePublic? OrganismePublic { get; set; }
        public Guid? PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage? Partage { get; set; }  //// Le partage n'existe pas forcément au moment de la dde d'infos --> le laisser en null?
        public Guid? AgentTraitantId { get; set; }
        [ForeignKey("AgentTraitantId")]
        public User? AgentTraitant { get; set; }

        //Faire une demande d'infor périmetre
        //--> Factory (= méthode de création) à utiliser si  :
        //- nécessite des règles métiers lors de la création
        //- on veut éviter d'avoir des objets dans un état invalide (ex. une demande d'infos périmètre sans partageId ou demandeurId)

        public static Result<DemandeGRD> CreateDemandeInfoPerimetre(
            Guid partageId,
            Guid demandeurId,
            string? detailsDemande = null)
            {
                if (partageId == Guid.Empty)
                return DemandeGRDErrors.PartageObligatoire().Map();

                 if (demandeurId == Guid.Empty)
                return DemandeGRDErrors.DemandeurObligatoire().Map();

                var demande = new DemandeGRD
                    {
                        Id = Guid.NewGuid(),
                        DateDemande = DateTime.UtcNow,
                        ResponseStatus = DdeGRDResponseStatus.EnAttente,
                        DemandeType = DemandeGRDType.DdeInfoPerimetre,
                        DemandeurId = demandeurId,
                        PartageId = partageId,
                        DetailsDemande = string.IsNullOrWhiteSpace(detailsDemande)
                            ? "Demande d'information de périmètre pour le partage."
                            : detailsDemande.Trim()
                    };

                    return Result.Success(demande);
            }

        // Réponse du GRD 
        public Result RepondreDemandePerimetre(
            PerimetreType perimetreConfirme,
            string? commentaireReponseGrd,
            Guid agentTraitantId,
            Guid? organismePublicId)
                    {
                        if (DemandeType != DemandeGRDType.DdeInfoPerimetre)
                            return DemandeGRDErrors.TypeDemandeInvalide();

                        if (ResponseStatus != DdeGRDResponseStatus.EnAttente)
                            return DemandeGRDErrors.DemandeDejaTraitee();

                        if (agentTraitantId == Guid.Empty)
                            return DemandeGRDErrors.AgentTraitantObligatoire();

                        PerimetreConfirme = perimetreConfirme;
                            if (string.IsNullOrWhiteSpace(commentaireReponseGrd))
                            { CommentaireReponseGRD = null;  }
                            else
                            { CommentaireReponseGRD = commentaireReponseGrd.Trim(); } //permet de ne pas garder des chaines vides ou composées uniquement d'espaces

            AgentTraitantId = agentTraitantId;
                        OrganismePublicId = organismePublicId;
                        DateReponse = DateTime.UtcNow;
                        ResponseStatus = DdeGRDResponseStatus.Valide;

                        return Result.Success();
        }
    }
}