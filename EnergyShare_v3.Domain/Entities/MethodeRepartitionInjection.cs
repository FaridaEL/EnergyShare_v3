using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EnergyShare_v3.Domain.Entities
{
    public class MethodeRepartitionInjection
    {
        /*La méthode de répartition peut évoluer dans le temps ou rester fixe pendant toute la durée du partage*/
           [Key]
        public Guid Id { get; set; }
        [Required]
        
        public DateOnly DateDebut { get; set; }
        public DateOnly DateFin {  get; set; }

        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; }

        //enumération
        public MethodeRepartitionInjectionType MethodeType { get; set; }

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }


}
