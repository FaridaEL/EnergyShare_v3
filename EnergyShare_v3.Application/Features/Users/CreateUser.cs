using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    public record CreateUserCommand(string Email,string PasswordHash,UserRole Role, UserType UserType);
    public class CreateUserHandler
    {
        private readonly IApplicationDbContext _context;

        public CreateUserHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> HandleAsync(
            CreateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = new User(command.Email, command.PasswordHash, command.Role, command.UserType );

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }

    }
}
