using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{
    /// <summary>
    /// Récupère le profil énergie de l'utilisateur connecté
    /// pour un point d'accès précis.
    /// Utile si l'utilisateur possède plusieurs points d'accès.
    /// </summary>
    public record GetMyProfilEnergieByPointAccessId(Guid PointAccessId)
        : IQuery<Result<ProfilEnergieDetailDto>>;

    public class GetMyProfilEnergieByPointAccessIdHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<GetMyProfilEnergieByPointAccessId, Result<ProfilEnergieDetailDto>>
    {
        public async ValueTask<Result<ProfilEnergieDetailDto>> Handle(
            GetMyProfilEnergieByPointAccessId query,
            CancellationToken cancellationToken)
        {
            // 1. Vérifier que l'utilisateur est authentifié.
            var userId = userContext.UserId;

            if (userId is null || userId == Guid.Empty)
                return Result<ProfilEnergieDetailDto>.Unauthorized();

            // 2. Vérifier que le point d'accès demandé appartient bien
            //    à l'utilisateur connecté.
            var pointAccessExists = await context.PointAccesses
                .AsNoTracking()
                .AnyAsync(
                    pa => pa.Id == query.PointAccessId
                       && pa.UserId == userId,
                    cancellationToken);

            if (!pointAccessExists)
            {
                return Result<ProfilEnergieDetailDto>.NotFound(
                    "Point d'accès introuvable.");
            }

            // 3. Récupérer le profil énergie lié à CE point d'accès.
            var profil = await context.ProfilsEnergie
                .AsNoTracking()
                .Where(pe => pe.PointAccessId == query.PointAccessId)
                .Select(pe => new ProfilEnergieDetailDto(
                    pe.Id,
                    pe.DemandeEnergie_kWh,
                    pe.OffreEnergie_kWh,
                    pe.PrixAchatCible_Eur,
                    pe.PrixVenteCible_Eur,
                    pe.PointAccessId,
                    userId.Value,
                    pe.Audit.CreatedAt,
                    pe.Audit.UpdatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);

            // 4. Le point appartient bien à l'utilisateur,
            //    mais aucun profil énergie n'existe encore.
            if (profil is null)
            {
                return Result<ProfilEnergieDetailDto>.NotFound(
                    "Aucun profil énergie n'est encore défini pour ce point d'accès.");
            }

            return Result.Success(profil);
        }
    }
}
