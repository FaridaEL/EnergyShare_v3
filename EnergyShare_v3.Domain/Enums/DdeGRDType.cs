using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum DdeGRDType
    {

        [Display(Name = "Nouvelle activation")]
        NouvelleActivation = 1,
        [Display(Name = "Modification d'un partage existant")]
        ModificationPartageExistant = 2,
        [Display(Name = "Cloture d'un partage existant")]
        CloturePartageExistant = 3

        
    }
}
