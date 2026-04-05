using EnergyShare_v3.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    //Récupérer tout les profils pour effectuer le matching et la simulation de l'économie d'énergie réalisée grâce au partage
    public record GetProfilsEnergieQuery;
    public class GetProfilsEnergieHandler
    {
        private readonly IApplicationDbContext _context;
        public GetProfilsEnergieHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ProfilEnergieSummaryDto>> HandleAsync(
            CancellationToken cancellationToken = default) {
            return await _context.ProfilsEnergie
                .AsNoTracking()
                .Select(pe => new ProfilEnergieSummaryDto(
                    pe.Id,
                    pe.DemandeEnergie_kWh,                      
                    pe.OffreEnergie_kWh,
                    pe.PrixAchatCible_Eur,
                    pe.PrixVenteCible_Eur,
                    pe.PointAccessId,
                    pe.PointAccess.UserId,     //scalaire très pratique pour récupérer l'info user utile
                    pe.PointAccess.User.Role,
                    pe.Audit.CreatedAt

                ) )
                .OrderBy(pe => pe.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    
    } 
}
