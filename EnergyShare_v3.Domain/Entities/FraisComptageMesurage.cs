using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class FraisComptageMesurage
    {
        /*Frais de comptage de mesure uniquement dus par l'acheteur annuellement
         On suppose que ces prix peuvent varier d'année en année
        actuellement de 12,78€HTVA*/
           [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Montant { get; set; }
        public DateOnly DateDebut { get; set; }
        public DateOnly DateFin {  get; set; }
        
        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; }

        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }


}
