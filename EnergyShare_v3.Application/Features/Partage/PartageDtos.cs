using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record PartageSummaryDto(
        Guid Id,
        string Nom,
        int NombreParticipants,
        DateTime CreatedAt
    );

    public record PartageDetailsDto(
        Guid Id,
        string Nom,
        string? Description,
        int NombreParticipants,
        DateTime? DateDebut,
        DateTime? DateFin,
        DateTime CreatedAt
    );
}
