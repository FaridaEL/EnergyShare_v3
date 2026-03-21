using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
