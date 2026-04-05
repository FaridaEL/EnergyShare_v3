using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{

    public record CreateProfilEnergie(
        decimal? DemandeEnergie_kWh,
        decimal? OffreEnergie_kWh,
        decimal? PrixAchatCible_Eur,
        decimal? PrixVenteCible_Eur,
        Guid PointAccessId
    ) : ICommand<Result<Guid>>;

    public class CreateProfilEnergieValidator : AbstractValidator<CreateProfilEnergie>
    {
        public CreateProfilEnergieValidator()    // vérifie les données d'entrée : guid vide, chaine vide, longeur max, format...
        {
            RuleFor(x => x.PointAccessId)
                .NotEmpty()
                .WithMessage("Le point d'accès est requis");
        }
    }

    public class CreateProfilEnergieHandler(IApplicationDbContext context)
        : ICommandHandler<CreateProfilEnergie, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(   //Orchestre : appelle le domaine, persiste, et retourne le résultat
            CreateProfilEnergie command,          // commande = intention métrier : créer un partage, ajouter un membre, etc.
            CancellationToken cancellationToken)
        {
            var result = Domain.Entities.ProfilsEnergie.ProfilEnergie.Create(
                command.DemandeEnergie_kWh,
                command.OffreEnergie_kWh,
                command.PrixAchatCible_Eur,
                command.PrixVenteCible_Eur,
                command.PointAccessId);

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var profil = result.Value;

            await context.ProfilsEnergie.AddAsync(profil, cancellationToken);

            return Result.Success(profil.Id);
        }
    }
}
