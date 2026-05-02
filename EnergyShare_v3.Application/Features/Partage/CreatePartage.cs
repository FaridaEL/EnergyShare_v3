using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using FluentValidation;
using Mediator;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record CreatePartage(
     string Nom,
     PartageEnergieType EnergieType//,
    // Guid VendeurId
 ) : ICommand<Result<Guid>>;

    public class CreatePartageValidator : AbstractValidator<CreatePartage>
    {
        public CreatePartageValidator()
        {
            RuleFor(x => x.Nom)
                .NotEmpty()
                .WithMessage("Le nom du partage est requis")
                .MaximumLength(100);

            /*
            RuleFor(x => x.VendeurId)
                .NotEmpty()
                .WithMessage("Le vendeur est requis");  */
        }
    }

    public class CreatePartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)   // on récupère l'Id du user connecté via IUserContext pour l'associer au partage créé
        : ICommandHandler<CreatePartage, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            CreatePartage command,
            CancellationToken cancellationToken)
        {
            // Vérifie qu’un utilisateur est bien connecté
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<Guid>.Unauthorized();

            var vendeurId = userContext.UserId.Value; // Utilise l'Id de l'utilisateur connecté comme vendeur du partage  via user-context

            var result = Domain.Entities.Partages.Partage.Create(
                command.Nom,
                command.EnergieType,
                vendeurId);

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var partage = result.Value;

            await context.Partages.AddAsync(partage, cancellationToken);

            return Result.Success(partage.Id);
        }
    }
}
