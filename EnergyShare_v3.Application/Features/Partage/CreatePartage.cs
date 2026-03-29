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
     PartageEnergieType EnergieType,
     DataTransmissionType DataTransmissionType,
     Guid VendeurId
 ) : ICommand<Result<Guid>>;

    public class CreatePartageValidator : AbstractValidator<CreatePartage>
    {
        public CreatePartageValidator()
        {
            RuleFor(x => x.Nom)
                .NotEmpty()
                .WithMessage("Le nom du partage est requis")
                .MaximumLength(100);

            RuleFor(x => x.VendeurId)
                .NotEmpty()
                .WithMessage("Le vendeur est requis");
        }
    }

    public class CreatePartageHandler(IApplicationDbContext context)
        : ICommandHandler<CreatePartage, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            CreatePartage command,
            CancellationToken cancellationToken)
        {
            var result = Domain.Entities.Partages.Partage.Create(
                command.Nom,
                command.EnergieType,
                command.DataTransmissionType,
                command.VendeurId);

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var partage = result.Value;

            await context.Partages.AddAsync(partage, cancellationToken);

            return Result.Success(partage.Id);
        }
    }
}
