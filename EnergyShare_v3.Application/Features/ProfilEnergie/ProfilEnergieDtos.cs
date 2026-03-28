using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    public record ProfilEnergieSummaryDto(
      Guid Id,
      decimal? DemandeEnergie_kWh,
      decimal? OffreEnergie_kWh,
      decimal? PrixAchatCible_Eur,
      decimal? PrixVenteCible_Eur,
      decimal? ConsommationAnnuelleEstime_kWh,
      decimal? ProductionAnnuelleEstime_kWh,
      decimal? PrixAchatEnergieFournisseur_Eur,
      decimal? PrixVenteInjectionFournisseurActuel_Eur,
      Guid PointAccessId,
      Guid UserId,
      UserRole UserRole,
      UserType UserType,
      DateTime CreatedAt
    );
}
