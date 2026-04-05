using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum UserRole
    {
        [Display(Name = "Utilisateur")]   //role standard pour vendre/acheter énergie/GestionnaireDePartage/profilEmployésSibelga
        Utilisateur = 1,                 
        [Display(Name = "Organisme public")]
        OrganismePublic = 2,
        [Display(Name = "Administrateur")]
        Administrateur = 3
    }
}
