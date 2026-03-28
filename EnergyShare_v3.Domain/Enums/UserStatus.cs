using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum UserStatus
    {
        [Display(Name = "Actif")]
        Actif = 1,
        [Display(Name = "Inactif")]
        Inactif = 2   
    }
}
