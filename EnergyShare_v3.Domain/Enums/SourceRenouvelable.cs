using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum SourceRenouvelable
    {
        [Display(Name = "Photovoltaique")]  // Le plus courant à Bruxelles
        Photovoltaique = 1,
        [Display(Name = "Cogénération")]    // uniquement communauté d'énergie
        Cogeneration = 2,
        [Display(Name = "Autres")]
        Autres = 3

    }
}
