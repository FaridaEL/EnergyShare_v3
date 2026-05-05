using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    // Query appelée par le GRD pour récupérer les ddes en attente de traitement.
    public record GetDemandesGrdEnAttente()
        : IQuery<Result<IReadOnlyList<DemandeGrdDto>>>;

    public class GetDemandesGrdEnAttenteHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<GetDemandesGrdEnAttente, Result<IReadOnlyList<DemandeGrdDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<DemandeGrdDto>>> Handle(
            GetDemandesGrdEnAttente query,
            CancellationToken cancellationToken)
        {
            // Seuls les utilisateurs authentifiés peuvent accéder à cette query.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<IReadOnlyList<DemandeGrdDto>>.Unauthorized();

            // Pour le MVP, on autorise Administrateur ou OrganismePublic.
            if (!userContext.IsInRole("Administrateur") && !userContext.IsInRole("OrganismePublic"))
            {
                return Result<IReadOnlyList<DemandeGrdDto>>.Forbidden();
            }

            // On récupère uniquement les ddes de périmètre en attente.
            // Le Partage est utilisé pour afficher le nom du partage dans l'UI GRD.
            var demandes = await context.DemandesGRD
                .AsNoTracking()
                .Where(d =>
                    d.DemandeType == DemandeGRDType.DdeInfoPerimetre &&
                    d.ResponseStatus == DdeGRDResponseStatus.EnAttente)
                .OrderBy(d => d.DateDemande)
                .Select(d => new DemandeGrdDto(
                    d.Id,
                    d.PartageId,
                    d.Partage != null ? d.Partage.Nom : null,
                    d.DateDemande,
                    d.DetailsDemande ?? string.Empty,
                    d.ResponseStatus,
                    d.DemandeType,
                    d.PerimetreConfirme,
                    d.CommentaireReponseGRD
                ))
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<DemandeGrdDto>>(demandes);
        }
    }
}