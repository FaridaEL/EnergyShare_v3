using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Ardalis.Result.FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

/*
 But : centraliser toutes les erreurs non gérées de l’application au même endroit.
 Sans lui, chaque endpoint/handler devrait avoir ses propres try/catch
 --> répétitif, incohérent et plus difficile à maintenir.

 Ce handler permet aussi :
 - de retourner des réponses HTTP propres au client ;
 - de logger les erreurs côté serveur ;
 - d’éviter d’exposer des détails techniques sensibles au client.
*/
namespace EnergyShare_v3.Web.Infrastructure
{
    /*ValidationBehavior jette une ValidationException.-> Ce handler la transforme en 400 Bad Request propre via Ardalis.Result.*/
    public class GlobalExceptionHandler : IExceptionHandler
    {
        /*  Le logger est injecté par Dependency Injection. Il permet de conserver l’erreur complète côté serveur
         sans renvoyer la stack trace au client.    */
        private readonly ILogger<GlobalExceptionHandler> _logger;

        private readonly IHostEnvironment _environment; //permet d'avoir plus de détails en développement (ex : stack trace complète) et moins d'infos en production.
        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync( //  TryHandleAsync est appelé automatiquement par ASP.NET Core lorsqu’une exception non gérée remonte dans le pipeline HTTP.
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            /*  On logge toujours l’exception complète côté serveur. Grâce au CorrelationIdMiddleware, ces logs sont reliés
              à une requête précise via le X-Correlation-ID.    */
            _logger.LogError(exception, "Une erreur non gérée est survenue.");

            //Cas 1 : erreur de validation FluentValidation. Ex : email invalide, champ obligatoire manquant, EAN incorrect, etc. */

            if (exception is FluentValidation.ValidationException validationException)
            {
                // Convertit les erreurs FluentValidation en Result.Invalid
                var errors = new ValidationResult(validationException.Errors)
                    .AsErrors();
                // Transforme Result.Invalid en réponse Minimal API propre
                var result = Result.Invalid(errors).ToMinimalApiResult();
                await result.ExecuteAsync(httpContext);
                return true;
            }
            // Cas 2 : autres erreurs connues --> On mappe certains types d’exceptions vers les bons codes HTTP.
            var problemDetails = exception switch
            {
                //Ressource introuvable. Ex : partage, user profil énergie demandé n’existe pas.
                KeyNotFoundException => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                    Title = "Ressource introuvable",
                    Detail = "La ressource demandée n'existe pas."
                },

                //Accès refusé.Ex : un user connecté tente une action réservée à un administrateur ou à un producteur/vendeur. 
                UnauthorizedAccessException => new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                    Title = "Accès refusé",
                    Detail = "Vous n'avez pas les droits nécessaires pour effectuer cette action."
                },

                // Erreur inattendue : On retourne un message volontairement générique au client pour ne pas exposer d’informations sensibles.
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError, // Erreur inattendue : 500 Internal Server Error
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    Title = "Erreur serveur",
                    //Detail = "Une erreur inattendue est survenue."
                    Detail = _environment.IsEnvironment("Testing")
                        ? exception.ToString()
                        : "Une erreur inattendue est survenue."
                }
            };

            // Définit le code HTTP de la réponse.
            httpContext.Response.StatusCode =
                problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
            // Format standard pour les erreurs d’API REST.
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(  // Écrit la réponse JSON renvoyée au client.
                    problemDetails,
                    cancellationToken);

            // true signifie : l’exception a été prise en charge-> ASP.NET Core ne doit donc pas continuer à chercher un autre handler.

            return true;
        }
    }
}

