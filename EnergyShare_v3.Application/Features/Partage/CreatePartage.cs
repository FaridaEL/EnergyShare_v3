using EnergyShare_v3.Application.Interfaces;
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

        public async Task<Guid> HandleAsync(
            CreatePartageCommand command,
            CancellationToken cancellationToken = default)
        {
            var partage = new Domain.Entities.Partage(
                command.Nom,
                command.EnergieType,
                command.DataTransmissionType,
                command.VendeurId
            );

            await _context.Partages.AddAsync(partage, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return partage.Id;
        }
    }
}
