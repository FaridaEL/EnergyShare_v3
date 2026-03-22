using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EnergyShare_v3.Domain.Entities
{
    public class TarifAccord
    {
        /*Tarif sur lequel le vendeur et l'acheteur se sont accordés
         Ce tarif peut évoluer chaque année ou  le long du partage ou être fixe tout le long du partagee*/
           [Key]
        public Guid Id { get; set; }
        [Required]
        public decimal Montant { get; set; }

        public DateOnly DateDebut { get; set; }
        public DateOnly DateFin {  get; set; }

        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; } = null!;

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }


}
