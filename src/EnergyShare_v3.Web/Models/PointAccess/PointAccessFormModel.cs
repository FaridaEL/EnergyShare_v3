using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Web.Models.PointAccess
{
    public class PointAccessFormModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "L'adresse est requise.")]
        public string AdresseLine1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code postal est requis.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Le code postal doit contenir 4 chiffres.")]
        public string CodePostal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le fournisseur est requis.")]
        public string Fournisseur { get; set; } = string.Empty;

        [RegularExpression(@"^1SJ.{0,17}$",ErrorMessage = "Le numéro de compteur doit commencer par 1SJ et contenir maximum 20 caractères.")]
        public string? SmartMeter { get; set; }

        [RegularExpression(@"^5414489\d{11}$",ErrorMessage = "Le code EAN doit commencer par 5414489 et contenir exactement 18 chiffres.")]
        public string? EAN { get; set; }

        public bool IsInjectionPoint { get; set; }
    }
}