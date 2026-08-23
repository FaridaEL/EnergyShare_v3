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
        string DetailsDemande,
        PerimetreType? PerimetreConfirme,  //permet d'afficher la réponse du GRD
        string? CommentaireReponseGRD
    );
    //TRaitement des demandes infos périmetre par le GRD
    public record DemandeGrdDto(
          Guid Id,
          Guid? PartageId,
          string? NomPartage,
          DateTime DateDemande,
          string DetailsDemande,
          DdeGRDResponseStatus ResponseStatus,
          DemandeGRDType DemandeType,
          PerimetreType? PerimetreConfirme,
          string? CommentaireReponseGRD
    );
    // Réponse GRD après traitement d'une dde d'info de périmètre .
    public record ReponseDemandePerimetreDto(
        Guid DemandeId,
        Guid? PartageId,
        PerimetreType PerimetreConfirme,
        string ResponseStatus,
        DateTime DateReponse,
        string? CommentaireReponseGRD
    );

    //Gestion des ddes de validation d'un nouveau partage
        public record DemandeValidationPartageDto(
        Guid DemandeId,
        Guid PartageId,
        DateTime DateDemande,
        string ResponseStatus,
        string DetailsDemande
    );


    //Réponse GRD 
    public record ReponseDemandeValidationPartageDto(
        Guid DemandeId,
        Guid PartageId,
        string ResponseStatus,
        PartageEnergieStatutType StatutPartage,
        DateTime DateReponse,
        string? CommentaireReponseGRD
    );

    // Gestion des demandes de modification d'un partage déjà actif
    public record DemandeModificationPartageDto(
        Guid DemandeId,
        Guid PartageId,
        DateTime DateDemande,
        string ResponseStatus,
        string DetailsDemande
    );

}
