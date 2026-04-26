using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Web.Models.ProfilEnergie
{
    public class ProfilEnergieFormModel
    {
        public Guid Id { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La demande ne peut pas être négative.")]
        public decimal? DemandeEnergie_kWh { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "L'offre ne peut pas être négative.")]
        public decimal? OffreEnergie_kWh { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le prix d'achat ne peut pas être négatif.")]
        public decimal? PrixAchatCible_Eur { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le prix de vente ne peut pas être négatif.")]
        public decimal? PrixVenteCible_Eur { get; set; }
    }
}
