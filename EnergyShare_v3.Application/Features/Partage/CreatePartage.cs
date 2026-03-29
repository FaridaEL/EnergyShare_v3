using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record CreatePartageCommand(
        string Nom,
        PartageEnergieType EnergieType,
        DataTransmissionType DataTransmissionType,
        Guid VendeurId
    );

    public class CreatePartageHandler
    {
        private readonly IApplicationDbContext _context;

        public CreatePartageHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> HandleAsync(    //on ajoute le result pour gérer les erreurs métier définies dans le domaine
            CreatePartageCommand command,
            CancellationToken cancellationToken = default)
        {
            
            var result = Domain.Entities.Partages.Partage.Create(
                command.Nom,
                command.EnergieType,
                command.DataTransmissionType,
                command.VendeurId
            );
            //Si erreur métier on s'arrête et on retourne l'erreur, sinon on continue
            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors); 

            var partage = result.Value;

            //persistance
            await _context.Partages.AddAsync(partage, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(partage.Id);
        }
    }
}
