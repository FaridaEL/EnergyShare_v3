using EnergyShare_v3.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EnergyShare_v3.Web.Endpoints
{
    public static class DebugEndpoint
    {
        public static IEndpointRouteBuilder MapDebug(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/debug").WithTags("Debug");

            group.MapGet("/me", GetMe)
                .RequireAuthorization(new AuthorizeAttribute
                {
                    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
                });

            return app;
        }

        private static IResult GetMe([FromServices] IUserContext userContext)
        {
            return Results.Ok(new
            {
                userContext.IsAuthenticated,
                userContext.UserId,
                userContext.Email,
                userContext.UserName,
                userContext.Roles,
                userContext.OrganismePublicId
            });
        }
    }
}
