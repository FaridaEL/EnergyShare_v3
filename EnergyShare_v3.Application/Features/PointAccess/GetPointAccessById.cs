using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.PointAccess
{
    public record GetPointAccessById(Guid Id)
        : IQuery<Result<PointAccessDetailDto>>;

    public class GetPointAccessByIdHandler(IApplicationDbContext context,
        IUserContext userContext) //permet de s'assurer que le user connecté consulte son point uniquement !)
        : IQueryHandler<GetPointAccessById, Result<PointAccessDetailDto>>
    {
        public async ValueTask<Result<PointAccessDetailDto>> Handle(
            GetPointAccessById query,
            CancellationToken cancellationToken)
        {
            var dto = await context.PointAccesses
                .AsNoTracking()
                .Where(pa => pa.Id == query.Id)
                .Select(pa => new PointAccessDetailDto(
                    pa.Id,
                    pa.AdresseLine1,
                    pa.CodePostal,
                    pa.Latitude,
                    pa.Longitude,
                    pa.IsInjectionPoint,
                    pa.Fournisseur,
                    pa.SmartMeter_Encrypted,
                    pa.EAN_Encrypted,
                    pa.AccordConsentement,
                    pa.EstActif,
                    pa.DesactiveAt,
                    pa.UserId,
                    pa.Audit.CreatedAt,
                    pa.Audit.UpdatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (dto is null)
                return Result<PointAccessDetailDto>.NotFound("Point d'accès introuvable.");


            var currentUserId = userContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<PointAccessDetailDto>.Unauthorized();

            if (dto.UserId != currentUserId && !userContext.IsInRole("Administrateur"))
                return Result<PointAccessDetailDto>.Forbidden();

            return Result.Success(dto);
        }
    }
}