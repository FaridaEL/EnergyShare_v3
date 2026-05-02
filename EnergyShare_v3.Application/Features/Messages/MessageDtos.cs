using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Messages
{
    public record MessageDto(
        Guid Id,
        string ObjetMessage,
        string Contenu,
        DateTime DateEnvoi,
        bool IsLu,
        Guid ExpediteurId,
        string? NomExpediteur,
        Guid DestinataireId,
        string? NomDestinataire,
        Guid? MatchId
);
}
