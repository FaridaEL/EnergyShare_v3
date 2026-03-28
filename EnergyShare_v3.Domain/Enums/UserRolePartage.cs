using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum UserRolePartage
    {
        [Display(Name = "Vendeur")]
        Vendeur = 1,
        [Display(Name = "Acheteur")]
        Acheteur = 2,
        [Display(Name = "Mixte")]
        Mixte = 3

    }
}
