using EnergyShare_v3.Application.Interfaces;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    public record CreateProfilEnergieCommand(
        decimal DemandeEnergie_kWh,
        decimal OffreEnergie_kWh,
        decimal PrixAchatCible_Eur,
        decimal PrixVenteCible_Eur,
        decimal ConsommationAnnuelleEstime_kWh,
        decimal ProductionAnnuelleEstime_kWh,
        decimal PrixAchatEnergieFournisseur_Eur,
        decimal PrixVenteInjectionFournisseurActuel_Eur,
        Guid PointAccessId
        );

    public class CreateProfilEnergieHandler
    {
        private readonly IApplicationDbContext _context;

        public CreateProfilEnergieHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        //cf. Ex complet 3.6 pour créer un partage 
        public async Task<Guid> HandleAsync(
            CreateProfilEnergieCommand command,
            CancellationToken cancellationToken = default)
        {
            
            var profilEnergie = new Domain.Entities.ProfilEnergie(
                command.DemandeEnergie_kWh,
                command.OffreEnergie_kWh,
                command.PrixAchatCible_Eur,
                command.PrixVenteCible_Eur,
                command.ConsommationAnnuelleEstime_kWh,
                command.ProductionAnnuelleEstime_kWh,
                command.PrixAchatEnergieFournisseur_Eur,
                command.PrixVenteInjectionFournisseurActuel_Eur,
                command.PointAccessId

                );

            await _context.ProfilsEnergie.AddAsync(profilEnergie, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return profilEnergie.Id;
        }

    }
}
