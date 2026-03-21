using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class DdeValidationPartage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime DateDemande { get; set; } = DateTime.UtcNow;

        public string? ReponseSibelga { get; set; }  // Champ libre pour que l'agent Sibelga puisse répondre.  
        public DateTime? DateReponseSibelga { get; set; }   //  RÉPONSE DE SIBELGA 
        public string? MotifRefusSibelga { get; set; }
        
      
        public Guid? PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage? Partage { get; set; } = null!;

        // StatutDemande : EnAttente, Valide, Refus
        // Gestion statut partage :
        // si type = nouvelleActivation valide --> statusPartage : "EnAttenteValidation" , si valide --> actif, si refus --> inactif
        // si type = ModificationPartageExistant --> StatusPartage devient EnAttenteModification, si valide --> actif, siRefus --> suspendu
        //Si type =   clôturePartage --> StatusPartage devient EnAttenteCloture, si valide --> clôturé, si refus --> actif

        //Enumérations
        public DdeGRDResponseStatus ResponseStatus { get; set; } = DdeGRDResponseStatus.EnAttente;
        public DdeGRDType? DemandeType { get; set; } // NouvelleActivation, ModificationPartageExistant, clôturePartage, etc.

         //Lien vers l'esace documentaire ou 1 ou pls document ?
        public DocumentPartage? PathConventionSignee { get; set; }   // On stocke le chemin vers le fichier (ex: /uploads/conventions/...) --> Mais si pls conventions?

        public Guid? OrganismePublicId { get; set; }
        [ForeignKey("OrganismePublicId")]
        public OrganismePublic? OrganismePublic { get; set; }
        public Guid? AgentTraitantId { get; set; }
        [ForeignKey("AgentTraitantId")]
        public User? AgentTraitant { get; set; }

    
    }
}