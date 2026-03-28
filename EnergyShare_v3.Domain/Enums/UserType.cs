using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum UserType
    {
        [Display(Name = "Professionnel")]
        Professionnel = 1,
        [Display(Name = "Résidentiel")]
        Residentiel = 2,
        [Display(Name = "Client protégé")]
        ClientProtege = 3
    }
}
