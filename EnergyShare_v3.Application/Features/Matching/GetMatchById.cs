using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Matching
{
    // Query de lecture : retourne un match enregistré par son Id pour l'user connecté
    public record GetMatchByIdQuery(Guid Id)
       : IQuery<Result<SavedMatchSummaryDto>>;

    public class GetMatchByIdHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<GetMatchByIdQuery, Result<SavedMatchSummaryDto>>
    {
        public async ValueTask<Result<SavedMatchSummaryDto>> Handle(
            GetMatchByIdQuery query,
            CancellationToken cancellationToken)
        {
            var currentUserId = userContext.UserId;

            if (currentUserId is null)
                return Result.Unauthorized();

            // Sécurité : on filtre directement en base
            var match = await context.Matches
                .AsNoTracking()
                .Where(m =>
                    m.Id == query.Id &&
                    (
                        m.PointAccessVendeur.UserId == currentUserId ||
                        m.PointAccessAcheteur.UserId == currentUserId
                    ))
                .Select(m => new SavedMatchSummaryDto(
                    m.Id,
                    m.PointAccessVendeurId,
                    m.PointAccessAcheteurId,

                     m.PointAccessVendeur.UserId,
                    m.PointAccessAcheteur.UserId,
                    m.PointAccessVendeur.User.FirstName ?? "Vendeur",
                    m.PointAccessAcheteur.User.FirstName ?? "Acheteur",

                    m.PointAccessVendeur.UserId == currentUserId
                        ? m.PointAccessAcheteur.UserId
                        : m.PointAccessVendeur.UserId,

                    m.PointAccessVendeur.UserId == currentUserId
                        ? (m.PointAccessAcheteur.User.FirstName ?? "Acheteur")
                        : (m.PointAccessVendeur.User.FirstName ?? "Vendeur"),

                    m.DistanceCalculee,
                    m.Audit.CreatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (match is null)
                return Result.NotFound("Match introuvable ou accès non autorisé.");

            return Result.Success(match);
        }
    }
}
