using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    public record GetUsersQuery;
    public class GetUsersHandler
    {
        private readonly IApplicationDbContext _context;

        public GetUsersHandler(IApplicationDbContext context)
        {
            _context = context;
        }

       public async Task<IReadOnlyList<UserSummaryDto>> HandleAsync(
       CancellationToken cancellationToken = default) 
        {
            return await _context.Users
                .AsNoTracking()
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email.Value,//Email est un value Object, on accède donc + facilement à sa valeur avec .Value
                    u.Role,
                    u.UserType,
                    u.Audit.CreatedAt
                ))
                .OrderBy(u => u.FirstName)
                .ToListAsync(cancellationToken); 
        }




    }
}
