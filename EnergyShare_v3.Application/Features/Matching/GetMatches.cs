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
            // =========================================================
            // 1. IDENTIFIER L'UTILISATEUR CONNECTÉ
            // =========================================================
            var currentUserId = currentUserContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result.Unauthorized();
           
            // =========================================================
            // 2. RÉCUPÉRER SES POINTS D'ACCÈS
            // =========================================================
            // La sécurité de la query repose sur ces identifiants :
            // un utilisateur ne peut consulter que les matchs dans lesquels l'un de ses propres points participe.
            var userPointAccessIds = await context.PointAccesses
                .AsNoTracking()
                .Where(pa => pa.UserId == currentUserId)
                .Select(pa => pa.Id)
                .ToListAsync(cancellationToken);

            // Aucun point d'accès = aucun match possible.
            if (!userPointAccessIds.Any())
                return Result.Success<IReadOnlyList<SavedMatchSummaryDto>>(
                    new List<SavedMatchSummaryDto>());

            // =========================================================
            // 3. RÉCUPÉRER LES MATCHS DE L'UTILISATEUR
            // =========================================================
            var matches = await context.Matches
                .AsNoTracking()
                // Sécurité : Le point vendeur OU le point acheteur doit appartenir à l'utilisateur connecté.
                .Where(m =>
                    userPointAccessIds.Contains(m.PointAccessVendeurId)
                    || userPointAccessIds.Contains(m.PointAccessAcheteurId))

                //  Les matchs les plus récents en premier.
                .OrderByDescending(m => m.Audit.CreatedAt)
                //Projection directe en DTO :  aucune entité Domain n'est exposée à l'UI.
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

                    // -------------------------
                    // Informations énergétiques
                    // -------------------------
                    // On conserve les informations des DEUX côtés.
                    // L'UI décidera ensuite d'afficher l'offre ou  la dde  selon le rôle de l'utilisateur.

                    m.PointAccessVendeur.ProfilEnergie != null
                        ? m.PointAccessVendeur.ProfilEnergie.OffreEnergie_kWh
                        : null,

                    m.PointAccessAcheteur.ProfilEnergie != null
                        ? m.PointAccessAcheteur.ProfilEnergie.DemandeEnergie_kWh
                        : null,

                    m.PointAccessVendeur.ProfilEnergie != null
                        ? m.PointAccessVendeur.ProfilEnergie.PrixVenteCible_Eur
                        : null,

                    m.PointAccessAcheteur.ProfilEnergie != null
                        ? m.PointAccessAcheteur.ProfilEnergie.PrixAchatCible_Eur
                        : null,


                    // -------------------------
                    // Disponibilité du CONTACT
                    // -------------------------
                    // Même logique que SearchPotentialMatches : un point n'est plus disponible s'il participe déjà à un partage.
                    // Si je suis vendeur -> vérifier le point acheteur.
                    // Si je suis acheteur -> vérifier le point vendeur.

                    m.PointAccessVendeur.UserId == currentUserId
                        ? !m.PointAccessAcheteur.Membres.Any()
                        : !m.PointAccessVendeur.Membres.Any(),


                    m.DistanceCalculee,
                    m.Audit.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<SavedMatchSummaryDto>>(matches);
        }
    }
}
