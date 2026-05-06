using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    // Commande envoyée par l'API / UI quand le GRD répond à une ddde de validation d'un nouveau partage.
    public record RepondreDemandeValidationPartage(
        Guid DemandeId,
        bool IsValide,
        string? CommentaireReponseGRD)
        : ICommand<Result<ReponseDemandeValidationPartageDto>>;

    public class RepondreDemandeValidationPartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<RepondreDemandeValidationPartage, Result<ReponseDemandeValidationPartageDto>>
    {
        public async ValueTask<Result<ReponseDemandeValidationPartageDto>> Handle(
            RepondreDemandeValidationPartage command,
            CancellationToken cancellationToken)
        {
            // 1. Vérifier qu'un utilisateur est connecté.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<ReponseDemandeValidationPartageDto>.Unauthorized();

            var userId = userContext.UserId.Value;

            // 2. Vérifier que l'utilisateur est autorisé à répondre : Pour le MVP : OrganismePublic ou Administrateur.
            if (!userContext.IsInRole("OrganismePublic") &&
                !userContext.IsInRole("Administrateur"))
            {
                return Result<ReponseDemandeValidationPartageDto>.Forbidden();
            }

            // 3. Charger la demande GRD avec son partage.
            // ! partage nécessaire pour faire évoluer son statut.
            var demande = await context.DemandesGRD
                .Include(d => d.Partage)
                    .ThenInclude(p => p!.Membres)
                .FirstOrDefaultAsync(d => d.Id == command.DemandeId, cancellationToken);

            if (demande is null)
                return Result<ReponseDemandeValidationPartageDto>.NotFound();

            if (demande.Partage is null)
                return Result<ReponseDemandeValidationPartageDto>.Invalid(new ValidationError(
                    "Partage",
                    "La demande GRD n'est liée à aucun partage.",
                    "DemandeGRD.PartageIntrouvable",
                    ValidationSeverity.Error));

            var partage = demande.Partage;

            // 4. Répondre à la demande GRD --> Cette méthode met à jour :
            // - le statut de la dde : Valide ou Refus
            // - la date de réponse
            // - l'agent traitant
            // - l'eventuel commentaire
            var reponseDemandeResult = demande.RepondreDemandeValidationPartage(
                command.IsValide,
                command.CommentaireReponseGRD,
                userId,
                userContext.OrganismePublicId);

            if (!reponseDemandeResult.IsSuccess)
                return Result<ReponseDemandeValidationPartageDto>.Invalid(
                    reponseDemandeResult.ValidationErrors);

            // 5. Faire évoluer le statut du partage selon la décision GRD.
            Result statutPartageResult;

            if (command.IsValide)
            { 
                statutPartageResult = partage.ValiderNouveauPartageParGrd();  // EnAttenteValidation -> Actif
            }
            else
            {
                statutPartageResult = partage.RefuserNouveauParGrd(); // EnAttenteValidation -> Inactif
            }

            if (!statutPartageResult.IsSuccess)
                return Result<ReponseDemandeValidationPartageDto>.Invalid(
                    statutPartageResult.ValidationErrors);

            // 6. Pas de SaveChangesAsync ici  --> UnitOfWorkBehavior sauvegarde automatiquement après le handler.

            return Result.Success(new ReponseDemandeValidationPartageDto(
                demande.Id,
                partage.Id,
                demande.ResponseStatus.ToString(),
                partage.Statut,
                demande.DateReponse!.Value,
                demande.CommentaireReponseGRD
            ));
        }
    }
}