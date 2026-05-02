using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{        /*Destiné éventuellement à l'admin :/*Todo implémenter logique CQRS/Mediator*/
    public record GetPartages
        : IQuery<Result<IReadOnlyList<PartageSummaryDto>>> ;

    public class GetPartagesHandler(IApplicationDbContext context)
        : IQueryHandler<GetPartages, Result<IReadOnlyList<PartageSummaryDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<PartageSummaryDto>>> Handle(
            GetPartages query,
            CancellationToken cancellationToken)
        {
            var partages = await context.Partages
                .AsNoTracking()
                // IMPORTANT :
                // Le tri doit se faire sur l'entité EF avant la projection vers le DTO.
                // Si on trie après le Select(new PartageSummaryDto(...)),
                // EF Core tente de traduire un tri sur un objet DTO construit en mémoire,
                // ce qui provoque une erreur 500.
                .OrderByDescending(p => p.Audit.CreatedAt)

                .Select(p => new PartageSummaryDto(
                    p.Id,
                    p.Nom,
                    p.EnergieType,
                    p.Statut,
                    p.Membres.Count(m => m.ExitAt == null),
                    p.Audit.CreatedAt
                ))
                //.OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);   
            return Result.Success<IReadOnlyList<PartageSummaryDto>>(partages);
        }
    }
}
