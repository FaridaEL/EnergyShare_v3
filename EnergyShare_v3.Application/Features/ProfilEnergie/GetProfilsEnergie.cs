using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    //Récupérer tout les profils pour effectuer le matching et la simulation de l'économie d'énergie réalisée grâce au partage
    public record GetProfilsEnergie
        : IQuery<Result<IReadOnlyList<ProfilEnergieSummaryDto>>>;
    public class GetProfilsEnergieHandler(IApplicationDbContext context)
        : IQueryHandler<GetProfilsEnergie, Result<IReadOnlyList<ProfilEnergieSummaryDto>>>
    {
        //private readonly IApplicationDbContext _context;
        //  public GetProfilsEnergieHandler(IApplicationDbContext context)
        // {            _context = context;}

        public async ValueTask<Result<IReadOnlyList<ProfilEnergieSummaryDto>>> Handle(
           GetProfilsEnergie query,
           CancellationToken cancellationToken)
        {
            var profils = await context.ProfilsEnergie
                .AsNoTracking()
                .Select(pe => new ProfilEnergieSummaryDto(
                    pe.Id,
                    pe.DemandeEnergie_kWh,                      
                    pe.OffreEnergie_kWh,
                    pe.PrixAchatCible_Eur,
                    pe.PrixVenteCible_Eur,
                    pe.PointAccessId,
                    pe.PointAccess.UserId,     //scalaire très pratique pour récupérer l'info user utile
                    pe.Audit.CreatedAt

                ) )
                .OrderBy(pe => pe.CreatedAt)
                .ToListAsync(cancellationToken);
            return Result.Success<IReadOnlyList<ProfilEnergieSummaryDto>>(profils);
        }
    
    } 
}
