using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Entities
{
    public class DocumentModele
    {
        /*Convention Type, fichier Excel de facturation et simulateur avancés à mettre à disposition des utilisateurs*/
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Titre { get; set; } = null!; // ex: "Convention Peer-to-Peer"
        public string? Description { get; set; } // ex: "À utiliser pour le partage entre deux voisins."
        public string? Format { get; set; }//Excel, Word, PDF, etc.

        [Required]
        public string TemplatePath { get; set; } = null!; // Chemin vers le fichier

        //Enumérations
        public DocumentType? DocumentType { get; set; }         

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }

   
}
