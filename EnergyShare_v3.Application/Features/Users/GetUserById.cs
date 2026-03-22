using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    public record GetUserByIdQuery;
    public class GetUserByIdQueryHandler
    {
        private readonly IEnergyShareDbContext _context;

        public GetUserByIdQueryHandler(IEnergyShareDbContext context)
        {
            _context = context;
        }

        //pas un bon exemple il faudrait plutot un partage et récupére le membre du partage cf. ex du prof en 3.5
        public async Task<IReadOnlyList<UserDetailsDto>> HandleAsync(
       CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Select(f => new UserDetailsDto(
                    f.Id,
                    f.FirstName,
                    f.CreatedAt
                ))
                .OrderBy(f => f.FirstName)
                .ToListAsync(cancellationToken);
        }




    }
}
