using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    // Commande envoyée par l'API / UI
    public record RepondreDemandePerimetre(
        Guid DemandeId,
        PerimetreType PerimetreConfirme,
        string? CommentaireReponseGRD)
        : ICommand<Result<ReponseDemandePerimetreDto>>;

    public class RepondreDemandePerimetreHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<RepondreDemandePerimetre, Result<ReponseDemandePerimetreDto>>
    {
        public async ValueTask<Result<ReponseDemandePerimetreDto>> Handle(
            RepondreDemandePerimetre command,
            CancellationToken cancellationToken)
        {
            // 1. Vérifier utilisateur connecté
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<ReponseDemandePerimetreDto>.Unauthorized();

            var userId = userContext.UserId.Value;

            // 2. Vérifier rôle GRD / Admin
            if (!userContext.IsInRole("OrganismePublic") && !userContext.IsInRole("Administrateur"))
                return Result<ReponseDemandePerimetreDto>.Forbidden();

            // 3. Charger la demande + partage associé
            var demande = await context.DemandesGRD
                .Include(d => d.Partage)
                .FirstOrDefaultAsync(d => d.Id == command.DemandeId, cancellationToken);

            if (demande is null)
                return Result<ReponseDemandePerimetreDto>.NotFound();

            var partage = demande.Partage;

            // 4. Appliquer la logique métier sur la demande
            var result = demande.RepondreDemandePerimetre(
                command.PerimetreConfirme,
                command.CommentaireReponseGRD,
                userId,
                userContext.OrganismePublicId // peut être null si admin
            );

            if (!result.IsSuccess)
                return Result<ReponseDemandePerimetreDto>.Invalid(result.ValidationErrors);

            // 🧠 . Mettre à jour le partage avec le périmètre confirmé
            var partageResult = partage.DefinirPerimetre(command.PerimetreConfirme);

            if (!partageResult.IsSuccess)
                return Result<ReponseDemandePerimetreDto>.Invalid(partageResult.ValidationErrors);

            // Pas de SaveChanges → UnitOfWorkBehavior s’en charge

            // 6. On retourne un DTO propre pour l’UI
            return Result.Success(new ReponseDemandePerimetreDto(
                demande.Id,
                partage.Id,
                demande.PerimetreConfirme!.Value,
                demande.ResponseStatus.ToString(),
                demande.DateReponse!.Value,
                demande.CommentaireReponseGRD
            ));
        }
    }
}
