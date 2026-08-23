using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    // Commande envoyée lorsque le GRD répond à une demande de modification d'un partage existant.
    public record RepondreDemandeModificationPartage(
        Guid DemandeId,
        bool IsValide,
        string? CommentaireReponseGRD)
        : ICommand<Result<ReponseDemandeModificationPartageDto>>;

    public class RepondreDemandeModificationPartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<RepondreDemandeModificationPartage, Result<ReponseDemandeModificationPartageDto>>
    {
        public async ValueTask<Result<ReponseDemandeModificationPartageDto>> Handle(
            RepondreDemandeModificationPartage command,
            CancellationToken cancellationToken)
        {
            // 1. Vérifier qu'un utilisateur est connecté.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<ReponseDemandeModificationPartageDto>.Unauthorized();

            var userId = userContext.UserId.Value;

            // 2. Vérifier que l'utilisateur est autorisé à répondre : Pour le MVP : OrganismePublic ou Administrateur.
            if (!userContext.IsInRole("OrganismePublic") && !userContext.IsInRole("Administrateur"))
            {
                return Result<ReponseDemandeModificationPartageDto>.Forbidden();
            }

            // 3. Charger la demande GRD avec son partage. --> ! partage nécessaire pour faire évoluer son statut.
            var demande = await context.DemandesGRD
                .Include(d => d.Partage)
                .FirstOrDefaultAsync(d => d.Id == command.DemandeId, cancellationToken);

            if (demande is null)
                return Result<ReponseDemandeModificationPartageDto>.NotFound();

            if (demande.Partage is null)
                return Result<ReponseDemandeModificationPartageDto>.Invalid(new ValidationError(
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
            var reponseDemandeResult = demande.RepondreDemandeModificationPartage(
                command.IsValide,
                command.CommentaireReponseGRD,
                userId,
                userContext.OrganismePublicId);

            if (!reponseDemandeResult.IsSuccess)
                return Result<ReponseDemandeModificationPartageDto>.Invalid(
                    reponseDemandeResult.ValidationErrors);

            // 5. Faire évoluer le statut du partage selon la décision GRD selon la décision du GRD : 
            //Valide : EnAttenteValidation -> Actif
            //Refusé : EnAttenteValidation -> Suspendu
            Result statutPartageResult;

            if (command.IsValide)
            { 
                statutPartageResult = partage.ValiderModificationPartageParGrd();  // EnAttenteValidation -> Actif
            }
            else
            {
                statutPartageResult = partage.RefuserModificationPartageParGrd(); // EnAttenteValidation -> Inactif
            }

            if (!statutPartageResult.IsSuccess)
                return Result<ReponseDemandeModificationPartageDto>.Invalid(
                    statutPartageResult.ValidationErrors);

            // 6. Pas de SaveChangesAsync ici  --> UnitOfWorkBehavior sauvegarde automatiquement après le handler.

            return Result.Success(new ReponseDemandeModificationPartageDto(
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