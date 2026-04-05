using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    public record ProfilEnergieSummaryDto(
      Guid Id,
      decimal? DemandeEnergie_kWh,
      decimal? OffreEnergie_kWh,
      decimal? PrixAchatCible_Eur,
      decimal? PrixVenteCible_Eur,
      Guid PointAccessId,
      Guid UserId,
      UserRole UserRole,
      DateTime CreatedAt
    );
}
