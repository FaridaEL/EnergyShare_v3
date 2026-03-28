using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum PerimetreType
    {
        [Display(Name = "A : Même batiment")]
        A = 1,
        [Display(Name = "B : Même cabine")]
        B = 2,
        [Display(Name = "C : Même poste Elia")]
        C = 3,
        [Display(Name = "D: Différents postes Elia")]
        D = 4

    }
}
