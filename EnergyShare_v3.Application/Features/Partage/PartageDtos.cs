using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record PartageSummaryDto(
        Guid Id,
        string Nom,
        PartageEnergieType EnergieType,
        PartageEnergieStatutType Statut,
        int NombreParticipants,
        DateTime CreatedAt
    );

    // doit permettre d'afficher le statut, la progression , savoir si l'user peut modifier  et afficher les infos clés du partage
    public record PartageDetailsDto(
        Guid Id,
        string Nom,
        string? Description,
        // Métier
        PartageEnergieType EnergieType,
        PartageEnergieStatutType Statut,

        // Participants
        int NombreParticipants,
        // Dates
        DateTime? DateDebut,
        DateTime? DateFin,
        DateTime CreatedAt,
        DateTime UpdatedAt,

        // Permissions UI
        bool CanEdit,        // permet de gérer l'accès à ce niveau et pas dans l'UI
        bool IsInterlocuteurUnique,

        // Progression (UI)
        int Progression ,

        //Demandes info périmetre 
        DemandePerimetreDto? DerniereDemandePerimetre
    );

    // Partage add member
    public record InvitationCodeDto(
        Guid PartageId,
        string InvitationCode,
        DateTime InvitationCodeExpiresAt
      );

    //Dde info périmetre GRD 
    public record DemandePerimetreDto(
        Guid DemandeId,
        Guid PartageId,
        DateTime DateDemande,
        string ResponseStatus,
        string DetailsDemande
    );



}
