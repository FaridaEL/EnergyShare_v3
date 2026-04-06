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
            var users = await _context.Users
               .AsNoTracking()
               .OrderBy(u => u.FirstName)
               .ThenBy(u => u.LastName)
               .ToListAsync(cancellationToken);

            return users
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email.Value,
                    u.Role,
                    u.Audit.CreatedAt
                ))
                .ToList();

            // Important : on récupère d'abord les entités en base (ToListAsync)
            // puis on fait la projection en mémoire (.Select)
            // Pourquoi ?
            // EF Core ne sait pas toujours traduire certaines expressions complexes en SQL comme l'accès à des Value Objects (ex: u.Email.Value).
            // Cela provoque une erreur à l'exécution (LINQ non traduisible).
            // Solution :
            // - on exécute la requête SQL simple (OrderBy + ToListAsync)
            // - puis on transforme en DTO côté C#
            // => plus robuste pour un MVP et évite les erreurs EF Core.


            /* Ancienne version avec erreur d'exécution : LINQ non traduisible à cause de l'accès à un Value Object (u.Email.Value) dans la projection.
             * return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email.Value,//Email est un value Object, on accède donc + facilement à sa valeur avec .Value
                    u.Role,
                    u.Audit.CreatedAt
                ))
                .ToListAsync(cancellationToken);    */
        }

    }
}
