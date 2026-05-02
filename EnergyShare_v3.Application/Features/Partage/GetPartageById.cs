using Ardalis.Result;
using EnergyShare_v3.Application.Features.PointAccess;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{              /*Todo implémenter logique CQRS/Mediator*/
    public record GetPartageById(Guid Id) : 
        IQuery<Result<PartageDetailsDto>>;

    public class GetPartageByIdHandler(
        IApplicationDbContext context,
        IUserContext userContext) :
        IQueryHandler<GetPartageById, Result<PartageDetailsDto>>
    {
        public async ValueTask<Result<PartageDetailsDto>> Handle(
            GetPartageById query,
            CancellationToken cancellationToken)
        {   // partage n'est pas lié directement à l'utilisateur -> on teste l'accès avant de projeter en DTO 
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<PartageDetailsDto>.Unauthorized();

            var currentUserId = userContext.UserId.Value;

            var isAdmin = userContext.IsInRole("Administrateur");

            var hasAccess = await context.Partages
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == query.Id &&
                    (
                        isAdmin ||
                        p.VendeurId == currentUserId ||
                        p.Membres.Any(m =>
                            m.ExitAt == null &&
                            m.PointAccess.UserId == currentUserId)
                    ),
                    cancellationToken);

            if (!hasAccess)
                return Result<PartageDetailsDto>.Forbidden();





            var dto = await context.Partages
                .AsNoTracking()
                .Where(p => p.Id == query.Id)
                .Select(p => new PartageDetailsDto(
                    p.Id,
                    p.Nom,
                    p.Description,
                    p.Membres.Count(m => m.ExitAt == null),
                    p.DateDebut,
                    p.DateFin,
                    p.Audit.CreatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if(dto is null)
            {
                return Result<PartageDetailsDto>.NotFound("Partage introuvable");
            }

            return Result.Success(dto);

            
        }
    }
}
