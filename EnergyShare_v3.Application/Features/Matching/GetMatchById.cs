using EnergyShare_v3.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Matching
{
    public record GetMatchByIdQuery(Guid Id);

    public class GetMatchByIdHandler
    {
        private readonly IApplicationDbContext _context;

        public GetMatchByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SavedMatchSummaryDto?> HandleAsync(
            GetMatchByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            return await _context.Matches
                .AsNoTracking()
                .Where(m => m.Id == query.Id)
                .Select(m => new SavedMatchSummaryDto(
                    m.Id,
                    m.PointAccessVendeurId,
                    m.PointAccessAcheteurId,
                    m.DistanceCalculee,
                    m.CreatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
