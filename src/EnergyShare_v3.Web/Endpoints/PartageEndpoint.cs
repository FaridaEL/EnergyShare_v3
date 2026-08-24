using ArdalisResultStatus = Ardalis.Result.ResultStatus;
using Ardalis.Result.AspNetCore;
using EnergyShare_v3.Application.Features.Partage;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.Web.Models.Partage;

namespace EnergyShare_v3.Web.Endpoints
{
    public static class PartageEndpoint
    {
        public static IEndpointRouteBuilder MapPartages(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/partages")
                .WithTags("Partages");

            var authenticatedUserPolicy = new AuthorizeAttribute
            {
                Policy = "AuthenticatedUser",
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            };
            var adminOnlyPolicy = new AuthorizeAttribute
            {
                Roles = "Administrateur",
                //Roles = "Administrateur,OrganismePublic"   //a implémenter plus tard
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            };
            //nécessaire pour afficher les ddes de périmètre en attente.
            var adminOrOrganismePublicPolicy = new AuthorizeAttribute
            {
                Roles = "Administrateur,OrganismePublic",
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            };


            // Accès réservé pour le moment à l'administrateur,
            // Plus tard, on pourra ajouter le rôle OrganismePublic/GRD
            // pour permettre la validation des dossiers de partage.
            group.MapGet("", GetPartages)
                .RequireAuthorization(adminOnlyPolicy);


            // GET /api/partages/my
            // Retourne les partages liés à l'utilisateur connecté.
            group.MapGet("/my", GetMyPartages)
                .RequireAuthorization(authenticatedUserPolicy);

            // Consulter le détail d’un partage --> handler vérifie si le user connecté a le droit d’y accéder.
            group.MapGet("/{id:guid}", GetPartageById)
                .RequireAuthorization(authenticatedUserPolicy);

            // POST /api/partages
            // Création d’un partage par l’utilisateur connecté : VendeurId ne vient PAS du front : il est récupéré via IUserContext dans le handler.
            group.MapPost("", CreatePartage)
                .RequireAuthorization(authenticatedUserPolicy);

            group.MapPut("/{id:guid}", UpdatePartage)
                .RequireAuthorization(authenticatedUserPolicy);


            // POST /api/partages/{id}/invitation-code
            // Retourne un code d’invitation valide pour le partage.
            // Le handler vérifie que l'utilisateur connecté est bien le créateur du partage.
            group.MapPost("/{id:guid}/invitation-code", GetInvitationCodePartage)
                .RequireAuthorization(authenticatedUserPolicy);

            // POST /api/partages/rejoindre
            // Permet à un utilisateur connecté de rejoindre un partage via un code d’invitation.
            group.MapPost("/rejoindre", RejoindrePartage)
                .RequireAuthorization(authenticatedUserPolicy);

            // POST /api/partages/{id}/demande-info-perimetre
            // Permet au vendeur/interlocuteur unique de demander les informations de périmètre au GRD.
            group.MapPost("/{id:guid}/demande-info-perimetre", DemandeInfoPerimetrePartage)
                .RequireAuthorization(authenticatedUserPolicy);

            //Gestion des ddes par le GRD 
            // GET /api/partages/demandes-grd/en-attente
            // Retourne les demandes GRD de périmètre en attente.
            // Accès réservé à l'administrateur ou à l'organisme public.
            group.MapGet("/demandes-grd/en-attente", GetDemandesGrdEnAttente)
                .RequireAuthorization(adminOrOrganismePublicPolicy);
            
            //Gestion des réponses Dde info périmetre par le GRD
            // POST /api/partages/demandes-grd/{id}/repondre
            // Permet à l'organisme public / GRD de répondre à une demande d'information de périmètre.
            group.MapPost("/demandes-grd/{id:guid}/repondre", RepondreDemandePerimetre)
                .RequireAuthorization(adminOrOrganismePublicPolicy);

            // POST /api/partages/{id}/demande-validation
            // Permet au vendeur de soumettre un nouveau partage au GRD.
            // Le handler vérifie   !!:
            // - nombre de participants
            // - périmètre confirmé
            // - absence de demande déjà en attente
            group.MapPost("/{id:guid}/demande-validation", DemandeValidationPartage)
                .RequireAuthorization(authenticatedUserPolicy);

            // POST /api/partages/demandes-grd/{id}/validation/repondre
            // Permet au GRD/admin de valider ou refuser une demande de validation d'un nouveau partage.
            group.MapPost("/demandes-grd/{id:guid}/validation/repondre", RepondreDemandeValidationPartage)
                .RequireAuthorization(adminOrOrganismePublicPolicy);

            // POST /api/partages/{id}/demande-modification
            // Permet au vendeur / interlocuteur unique de déclarer au GRD
            // les modifications apportées à un partage déjà actif.
            group.MapPost("/{id:guid}/demande-modification", DemandeModificationPartage)
                .RequireAuthorization(authenticatedUserPolicy);

            // POST /api/partages/demandes-grd/{id}/modification/repondre
            // Permet au GRD / organisme public de valider ou refuser
            // une demande de modification d'un partage déjà actif.
            group.MapPost(
                "/demandes-grd/{id:guid}/modification/repondre",
                RepondreDemandeModificationPartage)
                .RequireAuthorization(adminOrOrganismePublicPolicy);

            // GET /api/partages/{id}/historique-demandes-grd
            // Retourne l'ensemble des ddes GRD (attente,validées ou refusées) liées au partage 
            group.MapGet(
                "/{id:guid}/historique-demandes-grd",
                GetHistoriqueDemandesGrdPartage)
                .RequireAuthorization(authenticatedUserPolicy);


            return app;
        }

        internal static async Task<IResult> GetPartages(ISender sender)
        {
            var response = await sender.Send(new GetPartages());

            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> GetPartageById(
            ISender sender,
            Guid id)
        {
            var response = await sender.Send(new GetPartageById(id));

            if (response.Status == ArdalisResultStatus.Unauthorized)
                return Results.Unauthorized();

            if (response.Status == ArdalisResultStatus.Forbidden)
                return Results.StatusCode(403);

            if (response.Status == ArdalisResultStatus.NotFound)
                return Results.NotFound();

            if (!response.IsSuccess)
                return Results.BadRequest(response.Errors);

            return Results.Ok(response.Value);
        }

        internal static async Task<IResult> CreatePartage(
            ISender sender,
            [FromBody] CreatePartage command)
        {
            var response = await sender.Send(command);

            if (response.Status == ArdalisResultStatus.Unauthorized)
                return Results.Unauthorized();

            if (response.Status == ArdalisResultStatus.Forbidden)
                return Results.StatusCode(403);

            if (response.Status == ArdalisResultStatus.Invalid)
                return Results.BadRequest(response.ValidationErrors);

            if (!response.IsSuccess)
                return Results.BadRequest(response.Errors);

            // On retourne 201 Created avec l’id du partage créé.
            return Results.Created($"/api/partages/{response.Value}", response.Value);
        }

        internal static async Task<IResult> GetMyPartages(ISender sender)
        {
            var response = await sender.Send(new GetMyPartages());

            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> UpdatePartage(
            ISender sender,
            Guid id,
            [FromBody] UpdatePartageRequest request)
         {
            var response = await sender.Send(new UpdatePartage(
                id,
                request.Nom,
                request.Description,
                request.EnergieType,
                request.DateDebut,
                request.DateFin));

            return response.ToMinimalApiResult();
        }
        internal static async Task<IResult> GetInvitationCodePartage(
           ISender sender,
           Guid id)
        {
                var response = await sender.Send(new GetInvitationCodePartage(id));

                //return response.ToMinimalApiResult();
                if (response.Status == ArdalisResultStatus.Unauthorized)
                    return Results.Unauthorized();

                if (response.Status == ArdalisResultStatus.Forbidden)
                    return Results.StatusCode(403);

                if (response.Status == ArdalisResultStatus.NotFound)
                    return Results.NotFound();

                if (response.Status == ArdalisResultStatus.Invalid)
                    return Results.BadRequest(response.ValidationErrors);

                if (!response.IsSuccess)
                    return Results.BadRequest(response.Errors);

                return Results.Ok(response.Value);

        }

        internal static async Task<IResult> RejoindrePartage(
            ISender sender,
            [FromBody] RejoindrePartageRequest request)
        {
                var response = await sender.Send(new RejoindrePartage(request.InvitationCode));

                return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> DemandeInfoPerimetrePartage(
            ISender sender,
            Guid id)
        {
                var response = await sender.Send(new DemandeInfoPerimetrePartage(id));

                if (response.Status == ArdalisResultStatus.Unauthorized)
                    return Results.Unauthorized();

                if (response.Status == ArdalisResultStatus.Forbidden)
                    return Results.StatusCode(403);

                if (response.Status == ArdalisResultStatus.NotFound)
                    return Results.NotFound();

                if (response.Status == ArdalisResultStatus.Invalid)
                    return Results.BadRequest(response.ValidationErrors);

                if (!response.IsSuccess)
                    return Results.BadRequest(response.Errors);

                return Results.Ok(response.Value);
        }

        internal static async Task<IResult> GetDemandesGrdEnAttente(ISender sender)
        {
            var response = await sender.Send(new GetDemandesGrdEnAttente());

            if (response.Status == ArdalisResultStatus.Unauthorized)
                return Results.Unauthorized();

            if (response.Status == ArdalisResultStatus.Forbidden)
                return Results.StatusCode(403);

            if (!response.IsSuccess)
                return Results.BadRequest(response.Errors);

            return Results.Ok(response.Value);
        }

        //Réponse dde info périmètre par le GRD

        internal static async Task<IResult> RepondreDemandePerimetre(
            ISender sender,
            Guid id,
            [FromBody] RepondreDemandePerimetreRequest request)
            {
                var response = await sender.Send(new RepondreDemandePerimetre(
                    id,
                    request.PerimetreConfirme,
                    request.CommentaireReponseGRD));

                if (response.Status == ArdalisResultStatus.Unauthorized)
                    return Results.Unauthorized();

                if (response.Status == ArdalisResultStatus.Forbidden)
                    return Results.StatusCode(403);

                if (response.Status == ArdalisResultStatus.NotFound)
                    return Results.NotFound();

                if (response.Status == ArdalisResultStatus.Invalid)
                    return Results.BadRequest(response.ValidationErrors);

                if (!response.IsSuccess)
                    return Results.BadRequest(response.Errors);

                return Results.Ok(response.Value);
            }


        //Dde validation d'un nouveau partage 
        internal static async Task<IResult> DemandeValidationPartage(
            ISender sender,
            Guid id)
            {
                var response = await sender.Send(new DemandeValidationPartage(id));

                if (response.Status == ArdalisResultStatus.Unauthorized)
                    return Results.Unauthorized();

                if (response.Status == ArdalisResultStatus.Forbidden)
                    return Results.StatusCode(403);

                if (response.Status == ArdalisResultStatus.NotFound)
                    return Results.NotFound();

                if (response.Status == ArdalisResultStatus.Invalid)
                    return Results.BadRequest(response.ValidationErrors);

                if (!response.IsSuccess)
                    return Results.BadRequest(response.Errors);

                return Results.Ok(response.Value);
            }

        // Réponse du GRD à une demande de validation d'un nouveau partage
        internal static async Task<IResult> RepondreDemandeValidationPartage(
            ISender sender,
            Guid id,
            [FromBody] RepondreDemandeValidationPartageRequest request)
            {
                var response = await sender.Send(new RepondreDemandeValidationPartage(
                    id,
                    request.IsValide,
                    request.CommentaireReponseGRD));

                if (response.Status == ArdalisResultStatus.Unauthorized)
                    return Results.Unauthorized();

                if (response.Status == ArdalisResultStatus.Forbidden)
                    return Results.StatusCode(403);

                if (response.Status == ArdalisResultStatus.NotFound)
                    return Results.NotFound();

                if (response.Status == ArdalisResultStatus.Invalid)
                    return Results.BadRequest(response.ValidationErrors);

                if (!response.IsSuccess)
                    return Results.BadRequest(response.Errors);

                return Results.Ok(response.Value);
            }


        // Demande de modification d'un partage déjà actif.
        internal static async Task<IResult> DemandeModificationPartage(
            ISender sender,
            Guid id)
        {
            var response = await sender.Send(
                new DemandeModificationPartage(id));

            if (response.Status == ArdalisResultStatus.Unauthorized)
                return Results.Unauthorized();

            if (response.Status == ArdalisResultStatus.Forbidden)
                return Results.StatusCode(403);

            if (response.Status == ArdalisResultStatus.NotFound)
                return Results.NotFound();

            if (response.Status == ArdalisResultStatus.Invalid)
                return Results.BadRequest(response.ValidationErrors);

            if (response.Status == ArdalisResultStatus.Conflict)
                return Results.Conflict(response.Errors);

            if (!response.IsSuccess)
                return Results.BadRequest(response.Errors);

            return Results.Ok(response.Value);
        }

        // Réponse du GRD à une demande de modification
        // d'un partage déjà actif.
        internal static async Task<IResult> RepondreDemandeModificationPartage(
            ISender sender,
            Guid id,
            [FromBody] RepondreDemandeModificationPartageRequest request)
        {
            // Envoi de la commande vers la couche Application.
            // L'identifiant de la demande provient de l'URL,
            // tandis que la décision et le commentaire viennent du body HTTP.
            var response = await sender.Send(
                new RepondreDemandeModificationPartage(
                    id,
                    request.IsValide,
                    request.CommentaireReponseGRD));

            if (response.Status == ArdalisResultStatus.Unauthorized)
                return Results.Unauthorized();

            if (response.Status == ArdalisResultStatus.Forbidden)
                return Results.StatusCode(403);

            if (response.Status == ArdalisResultStatus.NotFound)
                return Results.NotFound();

            if (response.Status == ArdalisResultStatus.Invalid)
                return Results.BadRequest(response.ValidationErrors);

            if (!response.IsSuccess)
                return Results.BadRequest(response.Errors);

            return Results.Ok(response.Value);
        }


        // Historique complet des demandes GRD d'un partage.
        internal static async Task<IResult> GetHistoriqueDemandesGrdPartage(ISender sender, Guid id)
        {
            var response = await sender.Send( new GetHistoriqueDemandesGrdPartage(id));

            if (response.Status == ArdalisResultStatus.Unauthorized)
                return Results.Unauthorized();

            if (response.Status == ArdalisResultStatus.Forbidden)
                return Results.StatusCode(403);

            if (response.Status == ArdalisResultStatus.NotFound)
                return Results.NotFound();

            if (!response.IsSuccess)
                return Results.BadRequest(response.Errors);

            return Results.Ok(response.Value);
        }

    }
}
