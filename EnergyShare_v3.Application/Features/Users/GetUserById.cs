using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    public record GetUserByIdQuery(Guid Id);
    public class GetUserByIdHandler
    {
        private readonly IApplicationDbContext _context;

        public GetUserByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

       public async Task<UserDetailsDto?> HandleAsync(
           GetUserByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            /*💡 Note : Les queries utilisent toujours AsNoTracking() pour de meilleures performances et font de la projection (Select)
             * pour ne charger que les colonnes necessaires.*/
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == query.Id)
                .Select(u => new UserDetailsDto(
                    u.Id,
                    u.FirstName,
                    u.Audit.CreatedAt
                ))
                .FirstAsync(cancellationToken);
        }

    }
}
