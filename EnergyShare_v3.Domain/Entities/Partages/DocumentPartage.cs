using EnergyShare_v3.Bricks.Model;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.Partages
{
    public class DocumentPartage :IAuditable
    {

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string NomFichier { get; set; } = null!; // Nom d'origine (ex: Convention_A.pdf)

        [Required]
        public string CheminStockage { get; set; } = null!; // Chemin sur le serveur/Cloud
        public bool IsSigned { get; set; } = false;
        
        public DocumentType TypeDocument { get; set; } // Enum  Convention, Mandat, PreuvePropriete
        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; } = null!;

        //infos complémentaires sur qui a dépose le document
        public Guid UploadedById { get; set; }
        [ForeignKey("UploadedById")]
        public User UploadedBy { get; set; } = null!;

        ///Données d'audit
        public AuditInfo Audit { get; private set; } = new AuditInfo();

    }
   
}
