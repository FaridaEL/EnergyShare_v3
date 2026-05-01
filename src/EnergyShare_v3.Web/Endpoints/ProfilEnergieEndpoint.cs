using ArdalisResultStatus = Ardalis.Result.ResultStatus;
using Ardalis.Result.AspNetCore;
using EnergyShare_v3.Application.Features.ProfilEnergie;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyShare_v3.Web.Endpoints
{
    public static class ProfilEnergieEndpoint
    {
        public static IEndpointRouteBuilder MapProfilEnergie(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/profils-energie")
                .WithTags("ProfilsEnergie")
                .RequireAuthorization(new AuthorizeAttribute
                {
                    Policy = "AuthenticatedUser",
                    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                });

            group.MapGet("/me", GetMyProfilEnergie);
            group.MapGet("", GetProfilsEnergie);
            group.MapGet("/{id:guid}", GetProfilEnergieById);
            group.MapPost("", CreateProfilEnergie);
            group.MapPut("/{id:guid}", UpdateProfilEnergie);
            

            return app;
        }

        internal static async Task<IResult> GetProfilsEnergie(ISender sender)
        {
            var response = await sender.Send(new GetProfilsEnergie());
            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> GetProfilEnergieById(
        ISender sender,
        Guid id)
            {
                var response = await sender.Send(new GetProfilEnergieById(id));

                if (response.Status == ArdalisResultStatus.Forbidden)
                    return Results.StatusCode(403);

                if (response.Status == ArdalisResultStatus.NotFound)
                    return Results.NotFound();

                if (!response.IsSuccess)
                    return Results.BadRequest();

                return Results.Ok(response.Value);
        }

        internal static async Task<IResult> CreateProfilEnergie(
            ISender sender,
            [FromBody] CreateProfilEnergie command)
        {
            var response = await sender.Send(command);
            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> UpdateProfilEnergie(
            ISender sender,
            Guid id,
            [FromBody] UpdateProfilEnergieRequest request)
        {
            var response = await sender.Send(new UpdateProfilEnergie(
                id,
                request.DemandeEnergie_kWh,
                request.OffreEnergie_kWh,
                request.PrixAchatCible_Eur,
                request.PrixVenteCible_Eur));

            return response.ToMinimalApiResult();
        }


        internal static async Task<IResult> GetMyProfilEnergie(ISender sender)
        {
            var response = await sender.Send(new GetMyProfilEnergie());
            return response.ToMinimalApiResult();
        }
    }

    public record UpdateProfilEnergieRequest(
        decimal? DemandeEnergie_kWh,
        decimal? OffreEnergie_kWh,
        decimal? PrixAchatCible_Eur,
        decimal? PrixVenteCible_Eur
    );
}