using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.PointAccess
{
    /// <summary>
    /// Récupère tous les points d'accès appartenant à l'utilisateur connecté.
    /// Un utilisateur peut avoir plusieurs points d'accès.
    /// </summary>
    public record GetMyPointAccesses()
        : IQuery<Result<IReadOnlyList<PointAccessSummaryDto>>>;

    public class GetMyPointAccessesHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<GetMyPointAccesses, Result<IReadOnlyList<PointAccessSummaryDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<PointAccessSummaryDto>>> Handle(
            GetMyPointAccesses query,
            CancellationToken cancellationToken)
        {
            var userId = userContext.UserId;

            if (userId is null || userId == Guid.Empty)
                return Result<IReadOnlyList<PointAccessSummaryDto>>.Unauthorized();

            var pointsAccess = await context.PointAccesses
                .AsNoTracking()
                .Where(pa => pa.UserId == userId)
                .OrderByDescending(pa => pa.EstActif)
                .ThenByDescending(pa => pa.Audit.CreatedAt)
                .Select(pa => new PointAccessSummaryDto(
                    pa.Id,
                    pa.AdresseLine1,
                    pa.CodePostal,
                    pa.EAN_Encrypted,
                    pa.IsInjectionPoint,
                    pa.Fournisseur,
                    pa.EstActif,
                    pa.UserId,
                    pa.Audit.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<PointAccessSummaryDto>>(pointsAccess);
        }
    }
}