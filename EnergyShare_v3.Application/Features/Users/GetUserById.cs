      /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    using Ardalis.Result;
    using EnergyShare_v3.Application.Interfaces;
    using Mediator;
    using Microsoft.EntityFrameworkCore;

    namespace EnergyShare_v3.Application.Features.Users
    {
        public record GetUserByIdQuery(Guid Id)
            : IQuery<Result<UserDetailsDto>>;

        public class GetUserByIdHandler(IApplicationDbContext context)
            : IQueryHandler<GetUserByIdQuery, Result<UserDetailsDto>>
        {
            public async ValueTask<Result<UserDetailsDto>> Handle(
                GetUserByIdQuery query,
                CancellationToken cancellationToken)
        {    /*💡 Note : Les queries utilisent toujours AsNoTracking() pour de meilleures performances et font de la projection (Select)
          * pour ne charger que les colonnes necessaires.*/
            var user = await context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == query.Id)
                    .Select(u => new UserDetailsDto(
                        u.Id,
                        u.FirstName,
                        u.Audit.CreatedAt
                    ))
                    .FirstOrDefaultAsync(cancellationToken);

                if (user is null)
                    return Result<UserDetailsDto>.NotFound("Utilisateur introuvable.");

                return Result.Success(user);
            }
        }
    }
