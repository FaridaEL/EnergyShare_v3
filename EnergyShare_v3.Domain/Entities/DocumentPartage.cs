using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class DocumentPartage
    {

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string NomFichier { get; set; } = null!; // Nom d'origine (ex: Convention_A.pdf)

        [Required]
        public string CheminStockage { get; set; } = null!; // Chemin sur le serveur/Cloud
        [Required]
        public DateTime DateUpload { get; private set; } = DateTime.UtcNow;
        public bool IsSigned { get; set; } = false;
        public DateTime? SignedAt { get; set; }
        

        //Enumération
        public DocumentType TypeDocument { get; set; } // Enum  Convention, Mandat, PreuvePropriete

        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; } = null!;
        

        //infos complémentaires sur qui a dépose le document, qui l'a signé 
        public Guid UploadedById { get; set; }
        [ForeignKey("UploadedById")]
        public User UploadedBy { get; set; } = null!;

        /* hors MVP , de plus 1 document peut être signé par plusieurs signataires (ex: vendeur + acheteur) 
         * public Guid? SignedByNameId { get; set; } 
        [ForeignKey("SignedByNameId")]
        public User? SignedByName { get; set; }  */


        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;



    }

   
}
