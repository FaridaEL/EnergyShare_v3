namespace EnergyShare_v3.Web.Models.ProfilEnergie
{
    public record UpdateProfilEnergieRequest(
        decimal? DemandeEnergie_kWh,
        decimal? OffreEnergie_kWh,
        decimal? PrixAchatCible_Eur,
        decimal? PrixVenteCible_Eur
    );
}