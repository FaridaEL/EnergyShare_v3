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

        public string? SmartMeter { get; set; }

        public string? EAN { get; set; }

        public bool IsInjectionPoint { get; set; }
    }
}