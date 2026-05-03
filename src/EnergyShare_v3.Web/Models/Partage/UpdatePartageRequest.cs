using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Web.Models.Partage
{
    public record UpdatePartageRequest(
            string Nom,
            string? Description,
            PartageEnergieType EnergieType,
            DateTime? DateDebut,
            DateTime? DateFin
            );
}
