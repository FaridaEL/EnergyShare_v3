using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum MethodeRepartitionInjectionType
    {

        [Display(Name = "Fixe à un tour")]
        FixeUnTour = 1,
        [Display(Name = "Fixe à plusieurs tours")]
        FixePlusieursTours = 2,
        [Display(Name = "Dynamique à un tour : consommation au Prorata")]
        DynamiqueUnTourConsoProrata = 3,
        [Display(Name = "Dynamique à deux tours : Hybride")]
        DynamiqueDeuxToursHybride = 4
    }
}
