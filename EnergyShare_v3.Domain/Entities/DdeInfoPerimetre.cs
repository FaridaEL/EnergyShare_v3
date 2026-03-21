using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class DdeInfoPerimetre
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string AdressesMembres { get; set; } = null!;// idéalement à récupérer chez les users concernés  mais le partage n'existe pas encore forcément

        [Required]
        public DateTime DateDemande { get; set; } = DateTime.UtcNow;
        public DateTime? DateReponse { get; set; }    // Réponse de Sibelga
        public string? CommentaireSibelga { get; set; }

        //Enumérations
        public PerimetreType? PerimetreConfirme { get; set; } /// Réponse attendue : A, B, C ou D. 

        public Guid VendeurId { get; set; }   // Lien vers l'initiateur (le Vendeur)
        [ForeignKey("VendeurId")]
        public User Vendeur { get; set; } = null!;
        public Guid? PartageId { get; set; } // Le partage n'existe pas forcément au moment de la dde d'infos
        [ForeignKey("PartageId")]
        public Partage? Partage { get; set; }
        public Guid? OrganismePublicId { get; set; }
        [ForeignKey("OrganismePublicId")]
        public OrganismePublic? OrganismePublic { get; set; }

    }
}