using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum DdeGRDResponseStatus
    {

        [Display(Name = "Valide")]
        Valide = 1,
        [Display(Name = "Refus")]
        Refus = 2,
        [Display(Name = "En Attente")]
        EnAttente = 3

    }
}
