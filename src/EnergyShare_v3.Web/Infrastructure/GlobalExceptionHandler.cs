using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Ardalis.Result.FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;


namespace EnergyShare_v3.Web.Infrastructure
{
    /*ValidationBehavior jette une ValidationException.-> Ce handler la transforme en 400 Bad Request propre.*/
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is FluentValidation.ValidationException validationException)
            {
                // Convertit les erreurs FluentValidation en Result.Invalid
                var errors = new ValidationResult(validationException.Errors)
                    .AsErrors();
                var result = Result.Invalid(errors).ToMinimalApiResult();
                await result.ExecuteAsync(httpContext);
            }
            else
            {
                // Erreur inattendue : 500 Internal Server Error
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    Title = "Erreur serveur"
                };

                httpContext.Response.StatusCode = problemDetails.Status.Value;
                await httpContext.Response.WriteAsJsonAsync(
                    problemDetails, cancellationToken);
            }

            return true;
        }
    }
}
