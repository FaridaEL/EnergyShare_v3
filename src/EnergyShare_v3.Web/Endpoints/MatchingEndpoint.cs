using Ardalis.Result.AspNetCore;
using EnergyShare_v3.Application.Features.Matching;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyShare_v3.Web.Endpoints
{
    public static class MatchingEndpoint
    {
        public static IEndpointRouteBuilder MapMatching(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/matching")
                .WithTags("Matching")
                .RequireAuthorization(new AuthorizeAttribute
                {
                    Policy = "AuthenticatedUser",
                    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                });

            group.MapGet("/potential/{sourcePointAccessId:guid}", SearchPotentialMatches);
            group.MapPost("", CreateMatch);
            group.MapGet("", GetMatches);
            group.MapGet("/{id:guid}", GetMatchById);

            return app;
        }

        internal static async Task<IResult> SearchPotentialMatches(
            ISender sender,
            Guid sourcePointAccessId)
        {
            var response = await sender.Send(
                new SearchPotentialMatchesQuery(sourcePointAccessId));

            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> CreateMatch(
            ISender sender,
            [FromBody] CreateMatch command)
        {
            var response = await sender.Send(command);
            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> GetMatches(ISender sender)
        {
            var response = await sender.Send(new GetMatchesQuery());
            return response.ToMinimalApiResult();
        }

        internal static async Task<IResult> GetMatchById(
            ISender sender,
            Guid id)
        {
            var response = await sender.Send(new GetMatchByIdQuery(id));
            return response.ToMinimalApiResult();
        }
    }
}
