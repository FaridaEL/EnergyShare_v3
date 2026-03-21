using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
        public DateTime DateUpload { get; set; } = DateTime.UtcNow;
        public bool? IsSigned {  get; set; }
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
        public Guid? SignedByNameId { get; set; } // --> Mais si plusieurs signataires pas plus simple de laisser une ligne de texte.. comme un todo?
        [ForeignKey("SignedByNameId")]
        public User? SignedByName { get; set; }


        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;



    }

   
}
