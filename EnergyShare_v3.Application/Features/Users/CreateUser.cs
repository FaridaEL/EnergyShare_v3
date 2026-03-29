using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;

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
        public async Task<Result<Guid>> HandleAsync(
            CreateUserCommand command,
            CancellationToken cancellationToken = default)
        {    var result = Domain.Entities.Users.User.Create(
                command.Email,
                command.PasswordHash, 
                command.Role, 
                command.UserType );


            //Si erreur métier on s'arrête et on retourne l'erreur, sinon on continue
            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var user = result.Value;

            //persistance
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(user.Id);

        }

    }
}
