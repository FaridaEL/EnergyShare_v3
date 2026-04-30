//using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using EnergyShare_v3.Application.Features.PointAccess;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyShare_v3.Web.Endpoints
{
    public static class PointAccessEndpoint
    {
        public static IEndpointRouteBuilder MapPointAccess(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/points-access")
                .WithTags("PointAccess")
                .RequireAuthorization(new AuthorizeAttribute
                {
                    Policy = "AuthenticatedUser",
                    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                });

            group.MapGet("/me", GetMyPointAccesses);
            group.MapPost("/deactivate/{id:guid}", DeactivatePointAccess);
            group.MapPut("/{id:guid}", UpdatePointAccess);
            group.MapGet("/{id:guid}", GetPointAccessById);
            group.MapPost("", CreatePointAccess);

            group.MapGet("", GetPointAccesses)
                .RequireAuthorization(new AuthorizeAttribute
                {
                    Policy = "AdminOnly",
                    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                });

            return app;
        }

        internal static async Task<IResult> GetMyPointAccesses(ISender sender)
        {
            var response = await sender.Send(new GetMyPointAccesses());
            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> GetPointAccesses(ISender sender)
        {
            var response = await sender.Send(new GetPointAccesses());
            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> GetPointAccessById(
            ISender sender,
            Guid id)
        {
            var response = await sender.Send(new GetPointAccessById(id));
            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> CreatePointAccess(
            ISender sender,
            [FromBody] CreatePointAccess command)
        {
            var response = await sender.Send(command);
            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> UpdatePointAccess(
            ISender sender,
            Guid id,
            [FromBody] UpdatePointAccessRequest request)
        {
            var response = await sender.Send(new UpdatePointAccess(
                id,
                request.AdresseLine1,
                request.CodePostal,
                request.Fournisseur,
                request.SmartMeter,
                request.EAN,
                request.IsInjectionPoint));

            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> DeactivatePointAccess(
            ISender sender,
            Guid id)
        {
            var response = await sender.Send(new DeactivatePointAccess(id));
            //return response.ToMinimalApiResult();

            // Gestion explicite des réponses HTTP :
            // On n’utilise pas ToMinimalApiResult() ici car, avec Blazor Server + cookies,
            // les erreurs 401/403 sont automatiquement redirigées vers des pages HTML (login / access-denied).
            // Cela casse le comportement attendu d’une API (Swagger reçoit du HTML en 200).
            // On mappe donc manuellement les ResultStatus vers de vrais codes HTTP (401, 403, 404, etc.).
            return response.Status switch
            {
                Ardalis.Result.ResultStatus.Ok =>
                    Results.Ok(response.Value),

                Ardalis.Result.ResultStatus.Forbidden =>
                    TypedResults.StatusCode(StatusCodes.Status403Forbidden),  // TypedResults.* → retourne un vrai HTTP pur

                Ardalis.Result.ResultStatus.Unauthorized =>
                    Results.Unauthorized(),

                Ardalis.Result.ResultStatus.NotFound =>
                    Results.NotFound(),

                Ardalis.Result.ResultStatus.Conflict =>
                    Results.Conflict(response.Errors),

                Ardalis.Result.ResultStatus.Invalid =>
                    Results.BadRequest(response.ValidationErrors),

                _ =>
                    Results.BadRequest(response.Errors)
            };

        }
    }

    public record UpdatePointAccessRequest(
        string AdresseLine1,
        string CodePostal,
        string Fournisseur,
        string? SmartMeter,
        string? EAN,
        bool IsInjectionPoint
    );
}
