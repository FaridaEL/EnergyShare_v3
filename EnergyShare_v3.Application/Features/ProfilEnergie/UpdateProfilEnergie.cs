using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    public record UpdateProfilEnergie(
        Guid Id,
        decimal? DemandeEnergie_kWh,
        decimal? OffreEnergie_kWh,
        decimal? PrixAchatCible_Eur,
        decimal? PrixVenteCible_Eur
    ) : ICommand<Result<Guid>>;

    public class UpdateProfilEnergieValidator : AbstractValidator<UpdateProfilEnergie>
    {
        public UpdateProfilEnergieValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("L'identifiant du profil énergie est requis.");

            RuleFor(x => x)
                .Must(x => x.DemandeEnergie_kWh.HasValue || x.OffreEnergie_kWh.HasValue)
                .WithMessage("Une demande ou une offre d'énergie est requise.");

            RuleFor(x => x.DemandeEnergie_kWh)
                .GreaterThanOrEqualTo(0)
                .When(x => x.DemandeEnergie_kWh.HasValue);

            RuleFor(x => x.OffreEnergie_kWh)
                .GreaterThanOrEqualTo(0)
                .When(x => x.OffreEnergie_kWh.HasValue);

            RuleFor(x => x.PrixAchatCible_Eur)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PrixAchatCible_Eur.HasValue);

            RuleFor(x => x.PrixVenteCible_Eur)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PrixVenteCible_Eur.HasValue);
        }
    }

    public class UpdateProfilEnergieHandler(IApplicationDbContext context)
        : ICommandHandler<UpdateProfilEnergie, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            UpdateProfilEnergie command,
            CancellationToken cancellationToken)
        {
            var profil = await context.ProfilsEnergie
                .FirstOrDefaultAsync(pe => pe.Id == command.Id, cancellationToken);

            if (profil is null)
                return Result<Guid>.NotFound("Profil énergie introuvable.");

            var updateResult = profil.Update(
             command.DemandeEnergie_kWh,
             command.OffreEnergie_kWh,
             command.PrixAchatCible_Eur,
             command.PrixVenteCible_Eur);

            if (!updateResult.IsSuccess)
                return Result<Guid>.Invalid(updateResult.ValidationErrors);

            return Result.Success(profil.Id);
        }
    }
}