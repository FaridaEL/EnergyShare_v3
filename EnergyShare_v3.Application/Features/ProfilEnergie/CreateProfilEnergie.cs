using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
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

            RuleFor(x => x.DemandeEnergie_kWh)
                .GreaterThanOrEqualTo(0)
                .When(x => x.DemandeEnergie_kWh.HasValue)
                .WithMessage("La demande d'énergie ne peut pas être négative.");

            RuleFor(x => x.OffreEnergie_kWh)
                .GreaterThanOrEqualTo(0)
                .When(x => x.OffreEnergie_kWh.HasValue)
                .WithMessage("L'offre d'énergie ne peut pas être négative.");

            RuleFor(x => x.PrixAchatCible_Eur)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PrixAchatCible_Eur.HasValue)
                .WithMessage("Le prix d'achat cible ne peut pas être négatif.");

            RuleFor(x => x.PrixVenteCible_Eur)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PrixVenteCible_Eur.HasValue)
                .WithMessage("Le prix de vente cible ne peut pas être négatif.");

            RuleFor(x => x)
                .Must(x => x.DemandeEnergie_kWh.HasValue || x.OffreEnergie_kWh.HasValue)
                .WithMessage("Une demande ou une offre d'énergie est requise.");
        }
    }

    public class CreateProfilEnergieHandler(IApplicationDbContext context)
        : ICommandHandler<CreateProfilEnergie, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(   //Orchestre : appelle le domaine, persiste, et retourne le résultat
            CreateProfilEnergie command,          // commande = intention métrier : créer un partage, ajouter un membre, etc.
            CancellationToken cancellationToken)
        {
            // Vérifie que le point d'accès existe réellement.
            var pointAccessExists = await context.PointAccesses
                .AnyAsync(p => p.Id == command.PointAccessId, cancellationToken);

            if (!pointAccessExists)
                return Result<Guid>.NotFound("Le point d'accès est introuvable.");

            // Évite plusieurs profils énergie pour le même point d'accès.
            var profilAlreadyExists = await context.ProfilsEnergie
                .AnyAsync(p => p.PointAccessId == command.PointAccessId, cancellationToken);

            if (profilAlreadyExists)
                return Result<Guid>.Conflict("Un profil énergétique existe déjà pour ce point d'accès.");

            // Appelle la factory du domaine.
            var result = Domain.Entities.ProfilsEnergie.ProfilEnergie.Create(
                command.DemandeEnergie_kWh,
                command.OffreEnergie_kWh,
                command.PrixAchatCible_Eur,
                command.PrixVenteCible_Eur,
                command.PointAccessId);

            if (!result.IsSuccess)  // Si erreur métier, on retourne directement les erreurs.
                return Result<Guid>.Invalid(result.ValidationErrors);

            var profil = result.Value;

            await context.ProfilsEnergie.AddAsync(profil, cancellationToken);

            return Result.Success(profil.Id);
        }
    }
}
