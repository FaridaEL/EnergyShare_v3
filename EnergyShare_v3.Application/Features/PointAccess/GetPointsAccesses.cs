using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.PointAccess
{
    public record GetPointAccesses()
        : IQuery<Result<IReadOnlyList<PointAccessDetailDto>>>;

    public class GetPointAccessesHandler(IApplicationDbContext context)
        : IQueryHandler<GetPointAccesses, Result<IReadOnlyList<PointAccessDetailDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<PointAccessDetailDto>>> Handle(
            GetPointAccesses query,
            CancellationToken cancellationToken)
        {
            var list = await context.PointAccesses
                .AsNoTracking()
                .OrderByDescending(pa => pa.EstActif)
                .ThenBy(pa => pa.CodePostal)
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
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<PointAccessDetailDto>>(list);
        }
    }
}