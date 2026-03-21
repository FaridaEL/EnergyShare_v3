using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EnergyShare_v3.Domain.Entities
{
    public class DataPartage 
    {
            /*Doit correspondre aux champs du fichier sibelga qui est envoyé chaque mois avec les données réelles du partage
             Servira aussi au Dashboard statistique*/
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage PartageEnergie { get; set; } = null!;

        // Identifiant de la période (souvent un mois ou un quart d'heure selon  besoin)
        [Required]
        public DateTime DateDebut { get; set; }     //FromDate
        [Required]
        public DateTime DateFin { get; set; }      //ToDate

        public string?  EAN { get; set; }
        public string? Compteur { get; set; }
        
        public string? Partage { get; set; }

        public decimal Tarif { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? VolumePartage_kWh { get; set; } // "Volume local" dans le fichier Sibelga

        [Column(TypeName = "decimal(18,4)")]
        public decimal? VolumeComplementaire { get; set; } // "Volume local" dans le fichier Sibelga

        [Column(TypeName = "decimal(18,4)")]
        public decimal? InjectionPartage { get; set; } // Ce que le vendeur a produit au total

        public decimal GridfeeTotal { get; set; }

       
        
        // Métadonnées d'importation
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;



        // Données calculées (Getters)
        /* --> ces donénes seront calculés sur base de l'ensemble des tables 
        public decimal MontantVenteBrut => VolumePartage_kWh * GridfeeTotal;
        public decimal MontantFraisReseau => VolumePartage_kWh * GridfeeTotal;
        public decimal MontantTotalFacture => MontantVenteBrut + MontantFraisReseau;   */


    }

   
}
