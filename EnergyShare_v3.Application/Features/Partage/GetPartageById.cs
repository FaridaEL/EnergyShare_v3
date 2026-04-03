using EnergyShare_v3.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record GetPartageByIdQuery(Guid Id);

    public class GetPartageByIdHandler
    {
        private readonly IApplicationDbContext _context;

        public GetPartageByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PartageDetailsDto?> HandleAsync(
            GetPartageByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            return await _context.Partages
                .AsNoTracking()
                .Where(p => p.Id == query.Id)
                .Select(p => new PartageDetailsDto(
                    p.Id,
                    p.Nom,
                    p.Description,
                    p.Membres.Count(m => m.ExitAt == null),
                    p.DateDebut,
                    p.DateFin,
                    p.Audit.CreatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
