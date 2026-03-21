using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnergyShare_v3.Domain.Enums
{
    public enum DocumentType
    {
        [Display(Name = "Convention")]
        Convention = 1,
        [Display(Name = "Mandat")]
        Mandat = 2,
        [Display(Name = "Preuve de propriete")]
        PreuvePropriete = 3,
        [Display(Name = "Fichiers de données")]
        FichiersDonnées = 4,
        [Display(Name = "Autres")]
        Autres = 5

    }
}
