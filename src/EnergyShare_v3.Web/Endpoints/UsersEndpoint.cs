using Ardalis.Result.AspNetCore;
using EnergyShare_v3.Application.Features.Users;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyShare_v3.Web.Endpoints
{
    public static class UsersEndpoint
    {     /*On veut tester toute la chaine : HTTP → Endpoint → Mediator → Handler → Domain → Result → DB*/
        public static IEndpointRouteBuilder MapUsers(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users")
                .WithTags("Users");

            group.MapGet("", GetUsers) 
                .RequireAuthorization(new AuthorizeAttribute
                {
                    Policy = "AdminOnly", //mise en place authorisation : seuls les admins puevent voir la liste des users
                    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                }); 
            //group.MapPost("", CreateUser);
            //ajouter les autre après ici updateUser, etc.
            return app;
        }

        internal static async Task<IResult> GetUsers( [FromServices]GetUsersHandler handler)
        {
            var users = await handler.HandleAsync();
            return Results.Ok(users);
        }

        internal static async Task<IResult> CreateUser(
            [FromServices]ISender sender,
            [FromBody] CreateUser command)
        {
            var response = await sender.Send(command);
            return response.ToMinimalApiResult();
        }

        //Idem ici un internal static par endpoint
    }
}
