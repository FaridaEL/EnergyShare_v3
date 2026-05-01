using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Users;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    public record GetUsersQuery()
       : IQuery<Result<IReadOnlyList<UserSummaryDto>>>;
    public class GetUsersHandler (
        IApplicationDbContext context,
        UserManager<User> userManager) // on récupère le role depuis IDentityUserManager 
        : IQueryHandler<GetUsersQuery, Result<IReadOnlyList<UserSummaryDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<UserSummaryDto>>> Handle(
            GetUsersQuery query,
            CancellationToken cancellationToken)
        {
            var users = await context.Users
                .AsNoTracking()
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync(cancellationToken);

            var result = new List<UserSummaryDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Utilisateur";

                result.Add(new UserSummaryDto(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email ?? string.Empty,
                    role,
                    user.Audit.CreatedAt
                ));
            }

            return Result.Success<IReadOnlyList<UserSummaryDto>>(result);
        }        
    }

    }

