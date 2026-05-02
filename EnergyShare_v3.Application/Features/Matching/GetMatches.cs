using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Matching
{
    // Query de lecture : retourne les matchs du user connecté uniquement.
    public record GetMatchesQuery()
        : IQuery<Result<IReadOnlyList<SavedMatchSummaryDto>>>;

    public class GetMatchesHandler(
        IApplicationDbContext context,
        IUserContext currentUserContext)
        : IQueryHandler<GetMatchesQuery, Result<IReadOnlyList<SavedMatchSummaryDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<SavedMatchSummaryDto>>> Handle(
            GetMatchesQuery query,
            CancellationToken cancellationToken)
        {
            var currentUserId = currentUserContext.UserId;

            if (currentUserId is null)
                return Result.Unauthorized();


            // Points d'accès appartenant au user connecté.
            var userPointAccessIds = await context.PointAccesses
                .AsNoTracking()
                .Where(pa => pa.UserId == currentUserId)
                .Select(pa => pa.Id)
                .ToListAsync(cancellationToken);

            if (!userPointAccessIds.Any())
                return Result.Success<IReadOnlyList<SavedMatchSummaryDto>>(
                    new List<SavedMatchSummaryDto>());


            // Sécurité : un utilisateur ne voit que les matchs liés à ses points d’accès.
            var matches = await context.Matches
                .AsNoTracking()
                .Where(m =>
                    userPointAccessIds.Contains(m.PointAccessVendeurId)
                    || userPointAccessIds.Contains(m.PointAccessAcheteurId))

                // On trie sur l'entité EF avant la projection DTO.
                .OrderByDescending(m => m.Audit.CreatedAt)

                .Select(m => new SavedMatchSummaryDto(
                    m.Id,
                    m.PointAccessVendeurId,
                    m.PointAccessAcheteurId,



                    m.PointAccessVendeur.UserId,
                    m.PointAccessAcheteur.UserId,
                    m.PointAccessVendeur.User.FirstName ?? "Vendeur",
                    m.PointAccessAcheteur.User.FirstName ?? "Acheteur",

                    //opérateur ternaire if/else version courte si je suis vendeur je contacte un acheteur , sinon le contacte le vendeur
                    m.PointAccessVendeur.UserId == currentUserId
                        ? m.PointAccessAcheteur.UserId    
                        : m.PointAccessVendeur.UserId,

                    m.PointAccessVendeur.UserId == currentUserId
                        ? (m.PointAccessAcheteur.User.FirstName ?? "Acheteur")
                        : (m.PointAccessVendeur.User.FirstName ?? "Vendeur"),
                   
                    m.DistanceCalculee,
                    m.Audit.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<SavedMatchSummaryDto>>(matches);
        }
    }
}
