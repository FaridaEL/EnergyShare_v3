using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Web.Models.Partage
{
    public class PartageFormModel
    {
        [Required(ErrorMessage = "Le nom du partage est requis.")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public PartageEnergieType EnergieType { get; set; } = PartageEnergieType.PairToPair;
    }
}
