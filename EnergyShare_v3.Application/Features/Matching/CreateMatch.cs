using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.Matchs.Match;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Matching
{
    public record CreateMatchCommand(
        Guid PointAccessVendeurId,
        Guid PointAccessAcheteurId,
        decimal DistanceCalculee
    );

    public class CreateMatchHandler
    {
        private readonly IApplicationDbContext _context;

        public CreateMatchHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> HandleAsync(
            CreateMatchCommand command,
            CancellationToken cancellationToken = default)
        {
            // on vérifie s'il existe déjà un match pour ces points d'accès pour éviter d'enregistrer des doublons
            var existingMatch = await _context.Matches
                .FirstOrDefaultAsync(
                    m => m.PointAccessVendeurId == command.PointAccessVendeurId
                      && m.PointAccessAcheteurId == command.PointAccessAcheteurId,
                    cancellationToken);

            if (existingMatch is not null)
                return existingMatch.Id;

            var match = new Match(
                command.PointAccessVendeurId,
                command.PointAccessAcheteurId,
                command.DistanceCalculee
            );

            await _context.Matches.AddAsync(match, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return match.Id;
        }
    }

}