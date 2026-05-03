using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record GetMyPartages
        : IQuery<Result<IReadOnlyList<PartageSummaryDto>>>;

    public class GetMyPartagesHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<GetMyPartages, Result<IReadOnlyList<PartageSummaryDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<PartageSummaryDto>>> Handle(
            GetMyPartages query,
            CancellationToken cancellationToken)
        {
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<IReadOnlyList<PartageSummaryDto>>.Unauthorized();

            var currentUserId = userContext.UserId.Value;

            var partages = await context.Partages
                .AsNoTracking()
                .Where(p =>
                    p.VendeurId == currentUserId ||
                    p.Membres.Any(m =>
                        m.ExitAt == null &&
                        m.PointAccess.UserId == currentUserId))
                .OrderByDescending(p => p.Audit.CreatedAt)
                .Select(p => new PartageSummaryDto(
                    p.Id,
                    p.Nom,
                    p.EnergieType,
                    p.Statut,
                    p.Membres.Count(m => m.ExitAt == null),
                    p.Audit.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<PartageSummaryDto>>(partages);
        }
    }
}
