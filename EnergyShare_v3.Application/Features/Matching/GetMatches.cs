using EnergyShare_v3.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Matching
{
    public record GetMatchesQuery;
    public class GetMatchesHandler
    {
        private readonly IApplicationDbContext _context;
        public GetMatchesHandler(IApplicationDbContext context) { _context = context; }

        public async Task<IReadOnlyList<SavedMatchSummaryDto>> HandleAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Matches
                .AsNoTracking()
                .Select(m => new SavedMatchSummaryDto(
                    m.Id,
                    m.PointAccessVendeurId,
                    m.PointAccessAcheteurId,
                    m.DistanceCalculee,
                    m.Audit.CreatedAt
                ))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync(cancellationToken);

        }

    }
}
