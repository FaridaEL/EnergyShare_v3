using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnergyShare_v3.Domain.Enums
{
    public enum UserType
    {
        [Display(Name = "Professionnel")]
        Professionnel = 1,
        [Display(Name = "Résidentiel")]
        Residentiel = 2
    }
}
