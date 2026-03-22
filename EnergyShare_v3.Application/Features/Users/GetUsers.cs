using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    public record GetUsersQuery;
    public class GetUsersQueryHandler
    {
        private readonly IEnergyShareDbContext _context;

        public GetUsersQueryHandler(IEnergyShareDbContext context)
        {
            _context = context;
        }

       public async Task<IReadOnlyList<UserSummaryDto>> HandleAsync(
       CancellationToken cancellationToken = default) 
        {
            return await _context.Users
                .AsNoTracking()
                .Select(f => new UserSummaryDto(
                    f.Id,
                    f.FirstName,
                    f.LastName,
                    f.Email,
                    f.Role,
                    f.UserType,
                    f.CreatedAt
                ))
                .OrderBy(f => f.FirstName)
                .ToListAsync(cancellationToken); 
        }




    }
}
