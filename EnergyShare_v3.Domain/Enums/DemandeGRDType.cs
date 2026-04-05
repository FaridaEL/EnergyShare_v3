using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace EnergyShare_v3.Domain.Enums
{
    public enum DemandeGRDType
    {

        [Display(Name = "Nouvelle activation")]
        NouvelleActivation = 1,
        [Display(Name = "Modification d'un partage existant")]
        ModificationPartageExistant = 2,
        [Display(Name = "Cloture d'un partage existant")]
        CloturePartageExistant = 3,
        [Display(Name = "Demande information périmetre")]
        DdeInfoPerimetre = 4


    }
}
