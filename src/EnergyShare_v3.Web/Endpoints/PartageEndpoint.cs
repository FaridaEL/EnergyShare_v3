using ArdalisResultStatus = Ardalis.Result.ResultStatus;
using Ardalis.Result.AspNetCore;
using EnergyShare_v3.Application.Features.Partage;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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


            // Accès réservé pour le moment à l'administrateur,
            // Plus tard, on pourra ajouter le rôle OrganismePublic/GRD
            // pour permettre la validation des dossiers de partage.
            group.MapGet("", GetPartages)
                .RequireAuthorization(adminOnlyPolicy);

            
            // Consulter le détail d’un partage --> handler vérifie si le user connecté a le droit d’y accéder.
            group.MapGet("/{id:guid}", GetPartageById)
                .RequireAuthorization(authenticatedUserPolicy);

            // POST /api/partages
            // Création d’un partage par l’utilisateur connecté : VendeurId ne vient PAS du front : il est récupéré via IUserContext dans le handler.
            group.MapPost("", CreatePartage)
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
    }
}
