using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnergyShare_v3.Domain.Enums
{
    public enum UserRole
    {
        [Display(Name = "Vendeur")]
        Vendeur = 1,
        [Display(Name = "Acheteur")]
        Acheteur = 2,
        [Display(Name = "Gestionnaire de Partage")] 
        GestionnairePartage = 3,
        [Display(Name = "Organisme public")]
        OrganismePublic = 4,
        [Display(Name = "Administrateur")]
        Administrateur = 5
    }
}
