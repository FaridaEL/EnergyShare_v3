using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EnergyShare_v3.Domain.Entities
{
    public class ParametreSysteme
    {
        /*objectif de l'entité : l'admin peut ajouter des constantes pour effectuer des simulations de base. Ex : Si CO2_COEFF (coefficient de CO2) est de 0,15 kg/kWh 
         * et qu'on a partagé 100 kWh, alors l'application affichera sur le dashboard : "Vous avez évité l'émission de 15 kg de CO2 ce mois-ci".
        appliquer des valeurs moyennes. ex : Gridfee --> valeur moyenne de 0.03 cent**/
        [Key]
        public Guid Id { get; set; }
       
        [Required]
        public string Code { get; set; }   // ex: CO2_COEFF, Gridfee 
        public string? Description { get; set; }
        [Required]
        public string? TypeValeur { get; set; } //  decimal
        [Required]
        public string Unite { get; set; } //kWh, €
        public decimal Valeur { get; set; }  // ex : 0.03; 0,15

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt  { get; set; } = DateTime.UtcNow;


    }

   
}
