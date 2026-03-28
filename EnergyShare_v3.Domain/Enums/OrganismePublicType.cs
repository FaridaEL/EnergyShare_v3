using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum OrganismePublicType
    {

        [Display(Name = "Sibelga")]
        GRD = 1,
        [Display(Name = "Brugel")]
        Regulateur = 2
    }

}
