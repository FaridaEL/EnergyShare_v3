using Ardalis.Result.AspNetCore;
using EnergyShare_v3.Application.Features.Users;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyShare_v3.Web.Endpoints
{
    public static class UsersEndpoint
    {    /*On veut tester toute la chaine : HTTP → Endpoint → Mediator → Handler → Domain → Result → DB*/
        public static IEndpointRouteBuilder MapUsers(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users")
                .WithTags("Users");

            var authenticatedUserPolicy = new AuthorizeAttribute
            {
                Policy = "AuthenticatedUser",
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            };

            var adminOnlyPolicy = new AuthorizeAttribute
            {
                Policy = "AdminOnly",    //mise en place authorisation : seuls les admins puevent voir la liste des users
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            };

            group.MapGet("/me", GetMyProfile)
                .RequireAuthorization(authenticatedUserPolicy);

            group.MapPut("/me", UpdateMyProfile)
                .RequireAuthorization(authenticatedUserPolicy);

            group.MapGet("", GetUsers)
                .RequireAuthorization(adminOnlyPolicy);

            group.MapGet("/{id:guid}", GetUserById)
                .RequireAuthorization(adminOnlyPolicy);

            return app;
        }

        internal static async Task<IResult> GetUsers(
            [FromServices] ISender sender)
        {
            var result = await sender.Send(new GetUsersQuery());
            return result.ToMinimalApiResult();
        }

        internal static async Task<IResult> GetUserById(
            [FromServices] ISender sender,
            Guid id)
        {
            var result = await sender.Send(new GetUserByIdQuery(id));
            return result.ToMinimalApiResult();
        }

        internal static async Task<IResult> GetMyProfile(
            [FromServices] ISender sender)
        {
            var result = await sender.Send(new GetMyUserProfile());
            return result.ToMinimalApiResult();
        }

        internal static async Task<IResult> UpdateMyProfile(
            [FromServices] ISender sender,
            [FromBody] UpdateMyUserProfile command)
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        }
    }
}