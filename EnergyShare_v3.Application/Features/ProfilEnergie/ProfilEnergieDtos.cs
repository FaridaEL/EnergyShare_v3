using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    /// <summary>
    /// DTO de résumé utilisé pour :
    /// - affichage simple (liste, matching)
    /// - éviter d’exposer directement l’entité Domain
    /// </summary>
    public record ProfilEnergieSummaryDto(
        Guid Id,
        decimal? DemandeEnergie_kWh,
        decimal? OffreEnergie_kWh,
        decimal? PrixAchatCible_Eur,
        decimal? PrixVenteCible_Eur,
        Guid PointAccessId,
        Guid UserId,
        DateTime CreatedAt
    );

    /// <summary>
    /// DTO détaillé (optionnel pour plus tard)
    /// utile pour une page "Mon profil énergie"
    /// </summary>
    public record ProfilEnergieDetailDto(
        Guid Id,
        decimal? DemandeEnergie_kWh,
        decimal? OffreEnergie_kWh,
        decimal? PrixAchatCible_Eur,
        decimal? PrixVenteCible_Eur,
        Guid PointAccessId,
        Guid UserId,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    /// <summary>
    /// DTO pour update (utilisé dans UpdateProfilEnergie)
    /// </summary>
    public record UpdateProfilEnergieDto(
        decimal? DemandeEnergie_kWh,
        decimal? OffreEnergie_kWh,
        decimal? PrixAchatCible_Eur,
        decimal? PrixVenteCible_Eur
    );
}
