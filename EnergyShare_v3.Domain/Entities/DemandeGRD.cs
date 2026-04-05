using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
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

    
    }
}