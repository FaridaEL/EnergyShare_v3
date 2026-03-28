using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
///
/// Ce service orchestre les cas d'utilisation (use cases).
/// Il ne contient PAS de logique metier (ca, c'est dans le Domain).
/// Il coordonne : recoit une requete, appelle le domain, persiste le resultat.
///
/// Dans le Module 02 (CQRS), ce service sera remplace par des Commands/Queries.*/
    /*
    public class UserService(IApplicationDbContext context)
    {
        public async Task<IReadOnlyList<User>> GetAllUsersAsync()
        {
            return await context.Users
                .AsNoTracking() // Evite de tracker les entités pour une simple lecture, améliore les performances
                .OrderBy(u => u.CreatedAt) // Tri par date de création ou Prenom, Noms
                .ToListAsync();

        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> CreateUserAsync(string firstname, string lastname, string email, UserRole role)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstname,
                LastName = lastname,
                Role = role
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user is null)
                return false;
            return true;
        }
    }

*/

}
