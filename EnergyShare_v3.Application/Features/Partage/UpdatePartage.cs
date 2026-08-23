using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Enums;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record UpdatePartage(
        Guid Id,
        string Nom,
        string? Description,
        PartageEnergieType EnergieType,
        DateTime? DateDebut,
        DateTime? DateFin
    ) : ICommand<Result<Guid>>;

    public class UpdatePartageValidator : AbstractValidator<UpdatePartage>
    {
        public UpdatePartageValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("L'identifiant du partage est requis.");

            RuleFor(x => x.Nom)
                .NotEmpty()
                .WithMessage("Le nom du partage est requis.")
                .MaximumLength(100)
                .WithMessage("Le nom du partage ne peut pas dépasser 100 caractères.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("La description ne peut pas dépasser 1000 caractères.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x)
                .Must(x => !x.DateDebut.HasValue || !x.DateFin.HasValue || x.DateFin >= x.DateDebut)
                .WithMessage("La date de fin ne peut pas être antérieure à la date de début.");
        }
    }

    public class UpdatePartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<UpdatePartage, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            UpdatePartage command,
            CancellationToken cancellationToken)
        {
            var partage = await context.Partages
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            if (partage is null)
                return Result<Guid>.NotFound("Partage introuvable.");

            var currentUserId = userContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<Guid>.Unauthorized();

            // Pour le MVP, seul le vendeur créateur ou l'admin peut modifier le dossier. Plus tard, on ajoutera le gestionnaire de partage
            
            if (partage.VendeurId != currentUserId && !userContext.IsInRole("Administrateur"))
                return Result<Guid>.Forbidden();

            // Le partage est modifiable tant qu'il n'a pas été soumis au GRD.
            // Dès qu'il est en validation, actif, suspendu ou clôturé, on passe par des demandes de modification dédiées.
            //if (partage.Statut != PartageEnergieStatutType.Inactif)
            //    return Result<Guid>.Conflict("Le partage ne peut être modifié que lorsqu'il est inactif/brouillon.");
            // Le dossier ne peut pas être modifié pendant son examen initial par le GRD.
            if (partage.Statut == PartageEnergieStatutType.EnAttenteValidation)
            {
                return Result<Guid>.Conflict(
                    "Le partage ne peut pas être modifié pendant une demande de validation GRD en cours.");
            }

            // Une modification déjà déclarée au GRD doit être traitée avant d'en introduire une nouvelle.
            if (partage.Statut == PartageEnergieStatutType.EnAttenteModification)
            {
                return Result<Guid>.Conflict(
                    "Une demande de modification est déjà en attente de traitement par le GRD.");
            }

            // Un partage en cours de clôture ou clôturé n'est plus modifiable.
            if (partage.Statut == PartageEnergieStatutType.EnCoursCloture || partage.Statut == PartageEnergieStatutType.Cloture)
            {
                return Result<Guid>.Conflict(
                    "Le partage ne peut plus être modifié lorsqu'il est en cours de clôture ou clôturé.");
            }



            var result = partage.Update(
                command.Nom,
                command.Description,
                command.EnergieType,
                command.DateDebut,
                command.DateFin);

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            return Result.Success(partage.Id);
        }
    }
}