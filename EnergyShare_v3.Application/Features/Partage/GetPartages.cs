using EnergyShare_v3.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record GetPartagesQuery;

    public class GetPartagesHandler
    {
        private readonly IApplicationDbContext _context;

        public GetPartagesHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<PartageSummaryDto>> HandleAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Partages
                .AsNoTracking()
                .Select(p => new PartageSummaryDto(
                    p.Id,
                    p.Nom,
                    p.Membres.Count(m => m.ExitAt == null),
                    p.CreatedAt
                ))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
