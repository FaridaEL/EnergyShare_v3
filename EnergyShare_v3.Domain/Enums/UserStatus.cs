using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
