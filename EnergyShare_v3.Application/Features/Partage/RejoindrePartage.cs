using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    // Command appelée quand un utilisateur saisit un code d’invitation
    // pour rejoindre un partage existant.
    public record RejoindrePartage(string InvitationCode)
        : ICommand<Result<Guid>>;

    public class RejoindrePartageValidator : AbstractValidator<RejoindrePartage>
    {
        public RejoindrePartageValidator()
        {
            RuleFor(x => x.InvitationCode)
                .NotEmpty()
                .WithMessage("Le code d’invitation est requis.")
                .MaximumLength(32)
                .WithMessage("Le code d’invitation ne peut pas dépasser 32 caractères.");
        }
    }

    public class RejoindrePartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<RejoindrePartage, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            RejoindrePartage command,
            CancellationToken cancellationToken)
        {
            // Sécurité : seul un utilisateur connecté peut rejoindre un partage.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<Guid>.Unauthorized();

            var currentUserId = userContext.UserId.Value;

            // On normalise le code pour éviter les erreurs liées aux espaces ou aux minuscules/majuscules.
            var code = command.InvitationCode.Trim().ToUpperInvariant();

            // On recherche le partage associé au code.
            // Include(Membres) est utile car AjouterMembre agit sur la collection.
            var partage = await context.Partages
                //.Include(p => p.Membres)
                .FirstOrDefaultAsync(p => p.InvitationCode == code, cancellationToken);

            // Sécurité : Message volontairement simple afin ne pas exposer un détail sensible.
            if (partage is null)
            {
                return Result<Guid>.Invalid(
                    PartageErrors.InvitationCodeIntrouvable().ValidationErrors);
            }

            // Le code ne suffit pas !il doit également être encore valable.
            if (partage.InvitationCodeExpiresAt is null ||
                partage.InvitationCodeExpiresAt <= DateTime.UtcNow)
            {
                return Result<Guid>.Invalid(
                    PartageErrors.InvitationCodeExpire().ValidationErrors);
            }

            // On récupère le point d’accès du user connecté car la participation est liée à un PointAccess,
            // et pas directement au User.
            var pointAccess = await context.PointAccesses
                .FirstOrDefaultAsync(pa => pa.UserId == currentUserId, cancellationToken);

            if (pointAccess is null)
            {
                return Result<Guid>.Invalid(new ValidationError(
                    "PointAccess",
                    "Vous devez d’abord compléter votre point d’accès avant de rejoindre un partage.",
                    "RejoindrePartage.PointAccessObligatoire",
                    ValidationSeverity.Error));
            }

            // Règle métier : un même point d’accès ne peut pas appartenir à plusieurs partages actifs.
            var dejaDansUnPartageActif = await context.MembresPartage
                .AnyAsync(m =>
                    m.PointAccessId == pointAccess.Id &&
                    m.ExitAt == null &&
                    m.Partage.Statut != PartageEnergieStatutType.Cloture,
                    cancellationToken);

            if (dejaDansUnPartageActif)
            {
                return Result<Guid>.Invalid(
                    ParticipationPartageErrors
                        .PointAccessDejaDansUnPartageActif(pointAccess.Id)
                        .ValidationErrors);
            }

            // Création de la participation :Pour le MVP, l’utilisateur qui rejoint via code est considéré comme Acheteur.
            var participationResult = ParticipationPartage.Create(
                partage.Id,
                pointAccess.Id,
                UserRolePartage.Acheteur);

            if (!participationResult.IsSuccess)
                return Result<Guid>.Invalid(participationResult.ValidationErrors);

            // On ajoute la participation au partage via la méthode métier ongarde ainsi la logique dans le domaine 
            //  au lieu de manipuler directement les collections depuis le handler.
            var ajoutResult = partage.AjouterMembre(participationResult.Value);

            if (!ajoutResult.IsSuccess)
                return Result<Guid>.Invalid(ajoutResult.ValidationErrors);

            // Pas de SaveChangesAsync --> le UnitOfWorkBehavior sauvegarde automatiquement après le handler.

            // Important : on indique explicitement à EF que c’est une nouvelle participation.
            // Sinon EF peut parfois tenter un UPDATE au lieu d’un INSERT.
            context.MembresPartage.Add(participationResult.Value);

            // enfin on retourne l’Id du partage rejoint pour pouvoir rediriger l’utilisateur.
            return Result.Success(partage.Id);
        }
    }
}