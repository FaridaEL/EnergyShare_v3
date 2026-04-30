using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.PointAccess
{
    public record UpdatePointAccess(
        Guid Id,
        string AdresseLine1,
        string CodePostal,
        string Fournisseur,
        string? SmartMeter,
        string? EAN,
        bool IsInjectionPoint
    ) : ICommand<Result<Guid>>;

    public class UpdatePointAccessValidator : AbstractValidator<UpdatePointAccess>
    {
        public UpdatePointAccessValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("L'identifiant du point d'accès est requis.");

            RuleFor(x => x.AdresseLine1)
                .NotEmpty()
                .WithMessage("L'adresse est requise.");

            RuleFor(x => x.CodePostal)
                .NotEmpty()
                .WithMessage("Le code postal est requis.");

            RuleFor(x => x.Fournisseur)
                .NotEmpty()
                .WithMessage("Le fournisseur est requis.");
        }
    }

    public class UpdatePointAccessHandler(
        IApplicationDbContext context,
        IUserContext userContext) //permet de s'assurer que le user connecté modifie son point uniquement !)
        : ICommandHandler<UpdatePointAccess, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            UpdatePointAccess command,
            CancellationToken cancellationToken)
        {
            var entity = await context.PointAccesses
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (entity is null)
                return Result<Guid>.NotFound("Point d'accès introuvable.");

            var currentUserId = userContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<Guid>.Unauthorized();

            if (entity.UserId != currentUserId && !userContext.IsInRole("Administrateur"))
                return Result<Guid>.Forbidden();


            if (!entity.EstActif)
                return Result<Guid>.Conflict("Impossible de modifier un point d'accès désactivé.");

            // Règle applicative :
            // un même EAN ne peut pas être actif plusieurs fois en même temps.
            if (!string.IsNullOrWhiteSpace(command.EAN))
            {
                var eanAlreadyActive = await context.PointAccesses
                    .AnyAsync(pa =>
                        pa.Id != command.Id &&
                        pa.EAN_Encrypted == command.EAN.Trim() &&
                        pa.EstActif,
                        cancellationToken);

                if (eanAlreadyActive)
                    return Result<Guid>.Conflict(
                        "Ce code EAN est déjà rattaché à un autre point d'accès actif.");
            }

            var result = entity.Update(
                command.AdresseLine1,
                command.CodePostal,
                command.Fournisseur,
                command.SmartMeter,
                command.EAN,
                command.IsInjectionPoint);

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            return Result.Success(entity.Id);
        }
    }
}