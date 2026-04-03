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

    public class CreateMatchHandler(IApplicationDbContext context)
        : ICommandHandler<CreateMatch, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            CreateMatch command,
            CancellationToken cancellationToken)
        {
            var existingMatch = await context.Matches
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

            return Result.Success(match.Id);
        }
    }

}