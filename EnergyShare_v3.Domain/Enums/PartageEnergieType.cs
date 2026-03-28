using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum PartageEnergieType
    {
        [Display(Name = "Pair à Pair")]
        PairToPair = 1,
        [Display(Name = "Même batiment")]
        MemeBatiment = 2,
        [Display(Name = "Communaute d'énergie")]
        CommunauteEnergie = 3
     }
}
