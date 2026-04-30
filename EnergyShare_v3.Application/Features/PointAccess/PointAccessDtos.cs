namespace EnergyShare_v3.Application.Features.PointAccess
{
    public record PointAccessDetailDto(
      Guid Id,
      string? AdresseLine1,
      string? CodePostal,
      double? Latitude,
      double? Longitude,
      bool IsInjectionPoint,
      string Fournisseur,
      string? SmartMeter,
      string? EAN,
      bool AccordConsentement,
      bool EstActif,
      DateTime? DesactiveAt,
      Guid UserId,
      DateTime CreatedAt,
      DateTime? UpdatedAt
  );

    public record PointAccessSummaryDto(
        Guid Id,
        string? AdresseLine1,
        string? CodePostal,
        bool IsInjectionPoint,
        string Fournisseur,
        bool EstActif,
        Guid UserId,
        DateTime CreatedAt
    );
}
