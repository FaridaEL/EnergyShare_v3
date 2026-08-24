using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    /// <summary>
    /// Récupère l'ensemble des demandes GRD liées à un partage.
    ///
    /// Contrairement à GetDemandesGrdEnAttente, cette Query retourne aussi les demandes déjà traitées.
    /// Elle constitue donc l'historique administratif du partage.
    /// </summary>
    public record GetHistoriqueDemandesGrdPartage(Guid PartageId)
        : IQuery<Result<IReadOnlyList<HistoriqueDemandeGrdDto>>>;

    public class GetHistoriqueDemandesGrdPartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<
            GetHistoriqueDemandesGrdPartage,
            Result<IReadOnlyList<HistoriqueDemandeGrdDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<HistoriqueDemandeGrdDto>>> Handle(
            GetHistoriqueDemandesGrdPartage query,
            CancellationToken cancellationToken)
        {
            // 1. Vérifie que l'utilisateur est connecté.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
            {
                return Result<IReadOnlyList<HistoriqueDemandeGrdDto>>.Unauthorized();
            }

            var userId = userContext.UserId.Value;


            // 2. Vérifie que le partage existe.--> Pour rester simple, on charge uniquement les infos nécessaires au contrôle d'accès.
            var partage = await context.Partages
                .Include(p => p.Membres)
                .FirstOrDefaultAsync(
                    p => p.Id == query.PartageId,
                    cancellationToken);

            if (partage is null)
            {
                return Result<IReadOnlyList<HistoriqueDemandeGrdDto>>.NotFound();
            }
            // 3. Vérifie que l'utilisateur a le droit de consulter les démarches de ce partage.
            // Pour le MVP : l'historique est accessible :au vendeur / interlocuteur unique OU à l'administrateur.
            
            var canRead =  partage.VendeurId == userId || userContext.IsInRole("Administrateur");

            if (!canRead)
            {
                return Result<IReadOnlyList<HistoriqueDemandeGrdDto>>.Forbidden();
            }

            // 4. Récupère TOUTES les demandes liées au partage.
            // Pas de filtre sur ResponseStatus :   EnAttente, Valide et Refus font tous partie de l'historique.
            var demandes = await context.DemandesGRD
                .Where(d => d.PartageId == query.PartageId)
                .OrderByDescending(d => d.DateDemande)
                .Select(d => new HistoriqueDemandeGrdDto(
                    d.Id,
                    d.DateDemande,
                    d.DateReponse,
                    d.DemandeType,
                    d.ResponseStatus,
                    d.DetailsDemande ?? string.Empty,
                    d.CommentaireReponseGRD,
                    d.PerimetreConfirme
                ))
                .ToListAsync(cancellationToken);


            // 5. Retourne une liste vide si aucune dde n'existe.
            // Ce n'est pas une erreur : un nouveau partage peut simplement ne pas encore avoir de démarche GRD.
            return Result.Success<IReadOnlyList<HistoriqueDemandeGrdDto>>(demandes);
        }
    }
}