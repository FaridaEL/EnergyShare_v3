using ArdalisResultStatus = Ardalis.Result.ResultStatus;
using Ardalis.Result.AspNetCore;
using EnergyShare_v3.Application.Features.Messages;
using EnergyShare_v3.Application.Features.PointAccess;
using EnergyShare_v3.Application.Features.Users;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyShare_v3.Web.Endpoints
{
   
        public static class MessageEndpoint
        {    /*On veut tester toute la chaine : HTTP → Endpoint → Mediator → Handler → Domain → Result → DB*/
            public static IEndpointRouteBuilder MapMessages(this IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/messages")
                    .WithTags("Messages");

                var authenticatedUserPolicy = new AuthorizeAttribute
                {
                    Policy = "AuthenticatedUser",
                    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                };

                //var adminOnlyPolicy = new AuthorizeAttribute
                //{
                //    Policy = "AdminOnly",    //mise en place authorisation : seuls les admins puevent voir la liste des users
                //    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                //};

                group.MapGet("/inbox", GetInboxMessages)
                    .RequireAuthorization(authenticatedUserPolicy);
                
                group.MapGet("/outbox", GetSentMessages)
                    .RequireAuthorization(authenticatedUserPolicy);

                group.MapPost("/", SendMessage)
                    .RequireAuthorization(authenticatedUserPolicy);

                group.MapPut("/{id:guid}/read", MarkMessageAsRead)
                    .RequireAuthorization(authenticatedUserPolicy);

                return app;
            }

            internal static async Task<IResult> SendMessage(
            ISender sender,
            [FromBody] SendMessage command)
            {
                var response = await sender.Send(command);
            //return response.ToMinimalApiResult();
            if (response.Status == ArdalisResultStatus.Forbidden)
                return Results.StatusCode(403);

            if (response.Status == ArdalisResultStatus.NotFound)
                return Results.NotFound();

            if (!response.IsSuccess)
                return Results.BadRequest(response.Errors);

            return Results.Ok(response.Value);


        }
            internal static async Task<IResult> GetInboxMessages(
                [FromServices] ISender sender)
            {
                var result = await sender.Send(new GetInboxMessages());
                return result.ToMinimalApiResult();
            }

            internal static async Task<IResult> GetSentMessages(
               [FromServices] ISender sender)
            {
                var result = await sender.Send(new GetSentMessages());
                return result.ToMinimalApiResult();
            }

            internal static async Task<IResult> MarkMessageAsRead(
                ISender sender,                                      
                Guid id)             
            {
                var response = await sender.Send(new MarkMessageAsRead(id));
            //return response.ToMinimalApiResult();
            if (response.Status == ArdalisResultStatus.Forbidden)
                return Results.StatusCode(403);

            if (response.Status == ArdalisResultStatus.NotFound)
                return Results.NotFound();

            if (!response.IsSuccess)
                return Results.BadRequest(response.Errors);

            return Results.Ok();

        }
        
    }
}
