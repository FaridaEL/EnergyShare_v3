using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Matchs;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Matching
{     //Adoption d'une organisation par features (vertical slice) tout en gardant des DTO mutualisés
      //pour éviter la duplication et améliorer la lisibilité.

    public record CreateMatch(
        Guid PointAccessVendeurId,
        Guid PointAccessAcheteurId,
        decimal DistanceCalculee
    ) : ICommand<Result<Guid>>;

    public class CreateMatchValidator : AbstractValidator<CreateMatch>
    {
        public CreateMatchValidator()
        {
            RuleFor(x => x.PointAccessVendeurId)
                .NotEmpty()
                .WithMessage("Le point d'accès vendeur est requis.");

            RuleFor(x => x.PointAccessAcheteurId)
                .NotEmpty()
                .WithMessage("Le point d'accès acheteur est requis.");

            RuleFor(x => x.DistanceCalculee)
                .GreaterThanOrEqualTo(0)
                .WithMessage("La distance calculée doit être positive ou nulle.");
        }
    }

    public class CreateMatchHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<CreateMatch, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            CreateMatch command,
            CancellationToken cancellationToken)
        {
            var currentUserId = userContext.UserId;

            if (currentUserId is null)
                return Result.Unauthorized();

            // Sécurité : l'utilisateur connecté doit être l'un des deux points du match.
            var userOwnsOnePoint = await context.PointAccesses
                .AsNoTracking()
                .AnyAsync(pa =>
                    (pa.Id == command.PointAccessVendeurId ||
                     pa.Id == command.PointAccessAcheteurId)
                    && pa.UserId == currentUserId,
                    cancellationToken);

            if (!userOwnsOnePoint)
                return Result.Forbidden();

            // Cohérence : les deux points d'accès doivent exister.
            var pointsCount = await context.PointAccesses
                .AsNoTracking()
                .CountAsync(pa =>
                    pa.Id == command.PointAccessVendeurId ||
                    pa.Id == command.PointAccessAcheteurId,
                    cancellationToken);

            if (pointsCount != 2)
                return Result.NotFound("Un des points d'accès est introuvable.");

            var existingMatch = await context.Matches
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    m => m.PointAccessVendeurId == command.PointAccessVendeurId
                      && m.PointAccessAcheteurId == command.PointAccessAcheteurId,
                    cancellationToken);

            if (existingMatch is not null)
                return Result.Success(existingMatch.Id);

            var result = Match.Create(
                command.PointAccessVendeurId,
                command.PointAccessAcheteurId,
                command.DistanceCalculee
            );

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var match = result.Value;

            await context.Matches.AddAsync(match, cancellationToken);

            // Persistance réelle en base.
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(match.Id);
        }
    }

}