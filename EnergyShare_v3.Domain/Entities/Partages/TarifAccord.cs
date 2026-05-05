using EnergyShare_v3.Bricks.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.Partages
{
    public class TarifAccord   :IAuditable
    {
        /*Tarif sur lequel le vendeur et l'acheteur se sont accordés
         Ce tarif peut évoluer chaque année ou  le long du partage ou être fixe tout le long du partagee*/
           [Key]
        public Guid Id { get; set; }
        [Required]
        public decimal Montant { get; set; }

        public DateOnly DateDebut { get; set; }
        public DateOnly? DateFin {  get; set; }

        public Guid PartageId { get; set; }
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; } = null!;

        //Données d'audit
        public AuditInfo Audit { get; private set; } = new AuditInfo();


    }


}
