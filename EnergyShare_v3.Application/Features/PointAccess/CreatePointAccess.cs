using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.PointAccess
{
    public record CreatePointAccess(
        string AdresseLine1,
        string CodePostal,
        string Fournisseur,
        string? SmartMeter,
        string? EAN,
        bool IsInjectionPoint
    ) : ICommand<Result<Guid>>;

    public class CreatePointAccessValidator : AbstractValidator<CreatePointAccess>
    {
        public CreatePointAccessValidator()
        {
            RuleFor(x => x.AdresseLine1).NotEmpty();
            RuleFor(x => x.CodePostal).NotEmpty();
            RuleFor(x => x.Fournisseur).NotEmpty();
        }
    }

    public class CreatePointAccessHandler(
        IApplicationDbContext context,
        IUserContext userContext, 
        IGeocodingService geocodingService)
        : ICommandHandler<CreatePointAccess, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            CreatePointAccess command,
            CancellationToken cancellationToken)
        {
            var userId = userContext.UserId;

            if (userId is null || userId == Guid.Empty)
                return Result<Guid>.Unauthorized();


            // Règle applicative : un même EAN ne peut pas être actif plusieurs fois en même temps.
            if (!string.IsNullOrWhiteSpace(command.EAN))
            {
                var eanAlreadyActive = await context.PointAccesses
                    .AnyAsync(pa =>
                        pa.EAN_Encrypted == command.EAN.Trim()
                        && pa.EstActif,
                        cancellationToken);

                if (eanAlreadyActive)
                    return Result<Guid>.Conflict("Ce point d'accès est déjà rattaché à un utilisateur actif.");
            }

            // Géocodage de l'adresse via le service externe UrbIS.
            // L'Application ne connaît pas UrbIS directement : elle dépend uniquement de l'interface IGeocodingService.
            var geocodingResult = await geocodingService.GeocodeAsync(
                command.AdresseLine1,
                command.CodePostal,
                cancellationToken);

            // Si l'adresse n'a pas pu être localisée, on empêche la création du point d'accès.
            if (geocodingResult is null)
            {
                return Result<Guid>.Invalid( new ValidationError  {
                        ErrorMessage = "L'adresse n'a pas pu être localisée en Région bruxelloise."
                    });
            }


            var result = Domain.Entities.PointsAccesses.PointAccess.Create(
                userId.Value,
                command.AdresseLine1,
                command.CodePostal,
                command.Fournisseur,
                command.SmartMeter,
                command.EAN,
                command.IsInjectionPoint
            );

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var entity = result.Value;

            // Les coordonnées retournées par UrbIS sont enregistrées dans le point d'accès.
            entity.SetCoordinates( geocodingResult.Latitude, geocodingResult.Longitude);

            await context.PointAccesses.AddAsync(entity, cancellationToken);

            return Result.Success(entity.Id);
        }
    }
}