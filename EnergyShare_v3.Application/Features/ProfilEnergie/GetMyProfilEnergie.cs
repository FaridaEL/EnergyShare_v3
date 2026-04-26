using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    /// <summary>
    /// Récupère le profil énergie de l'utilisateur connecté.
    /// Utile pour la page "Mon profil énergie".
    /// </summary>
    public record GetMyProfilEnergie()
        : IQuery<Result<ProfilEnergieDetailDto>>;

    public class GetMyProfilEnergieHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<GetMyProfilEnergie, Result<ProfilEnergieDetailDto>>
    {
        public async ValueTask<Result<ProfilEnergieDetailDto>> Handle(
            GetMyProfilEnergie query,
            CancellationToken cancellationToken)
        {
            // 1. Vérifie que l'utilisateur est bien connecté
            var userId = userContext.UserId;

            if (userId is null || userId == Guid.Empty)
                return Result<ProfilEnergieDetailDto>.Unauthorized();


            // Diagnostic : est-ce que l'app voit bien le PointAccess de Sarah ?
            var pointAccessCount = await context.PointAccesses
                .AsNoTracking()
                .CountAsync(pa => pa.UserId == userId, cancellationToken);

            // Diagnostic : est-ce que l'app voit bien le profil lié à ce PointAccess ?
            var profilCount = await (
                from pe in context.ProfilsEnergie.AsNoTracking()
                join pa in context.PointAccesses.AsNoTracking()
                    on pe.PointAccessId equals pa.Id
                where pa.UserId == userId
                select pe
            ).CountAsync(cancellationToken);



            // 2. Cherche le profil énergie lié à son point d'accès
            /*var profil = await context.ProfilsEnergie
                .AsNoTracking()
                .Where(pe => pe.PointAccess.UserId == userId)
                .Select(pe => new ProfilEnergieDetailDto(
                    pe.Id,
                    pe.DemandeEnergie_kWh,
                    pe.OffreEnergie_kWh,
                    pe.PrixAchatCible_Eur,
                    pe.PrixVenteCible_Eur,
                    pe.PointAccessId,
                    pe.PointAccess.UserId,
                    pe.Audit.CreatedAt,
                    pe.Audit.UpdatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);  */
            var profil = await (
               from pe in context.ProfilsEnergie.AsNoTracking()
               join pa in context.PointAccesses.AsNoTracking()
                   on pe.PointAccessId equals pa.Id
               where pa.UserId == userId
               select new ProfilEnergieDetailDto(
                   pe.Id,
                   pe.DemandeEnergie_kWh,
                   pe.OffreEnergie_kWh,
                   pe.PrixAchatCible_Eur,
                   pe.PrixVenteCible_Eur,
                   pe.PointAccessId,
                   pa.UserId,
                   pe.Audit.CreatedAt,
                   pe.Audit.UpdatedAt
               )
            ).FirstOrDefaultAsync(cancellationToken);


            if (profil is null)
                return Result<ProfilEnergieDetailDto>.NotFound(
                    $"Aucun profil énergie trouvé. DEBUG userId={userId}, pointAccessCount={pointAccessCount}, profilCount={profilCount}");

            return Result.Success(profil);
        }
    }
}
