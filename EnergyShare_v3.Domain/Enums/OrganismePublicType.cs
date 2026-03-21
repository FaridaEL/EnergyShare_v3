using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
