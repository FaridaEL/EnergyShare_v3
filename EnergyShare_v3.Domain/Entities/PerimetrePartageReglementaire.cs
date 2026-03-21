using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities
{
    public class PerimetrePartageReglementaire

        /*A compléter par l'administrateur qui pourra indiquer les tarifs réseau réglementaire applicable
         On ne prévoir pas de table historique car meme batiment d'office A
        Pair to pair --> 2 personnes si l'un quitte le partage est terminé
        L'historique peut -etre intéressant dans le cas d'une communauté d'énergie. --> mais dans une V2*/
    {
        [Key]
        public Guid Id { get; set; }
        
        public string? Description { get; set; }  //meme batiement, meme cabine, meme quartier, etc.
        public DateOnly DateDebut { get; set; }  //ex  01/01/2026
        public DateOnly? DateFin { get; set; }   // ex 31/12/2026
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontantTarifReseau { get; set; } // tarif en €/kWh pour le partage d'énergie, 

        //enumération
        public PerimetreType Perimetre { get; set; }
       
        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}