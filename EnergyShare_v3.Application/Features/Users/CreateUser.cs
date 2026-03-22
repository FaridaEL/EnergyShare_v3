using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    public record CreateUserCommand;
    public class CreateUserHandler
    {
        private readonly IEnergyShareDbContext _context;

        public CreateUserHandler(IEnergyShareDbContext context)
        {
            _context = context;
        }
        //cf. Ex complet 3.6 pour créer un partage 
        public async Task<Guid> HandleAsync(
            CreateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            // La validation est dans le constructeur de l'entite Family
            var user = new User();

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }


    }
}
