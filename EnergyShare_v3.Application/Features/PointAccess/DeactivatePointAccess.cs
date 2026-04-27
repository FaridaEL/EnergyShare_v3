using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.PointAccess
{
    public record DeactivatePointAccess(Guid Id)
        : ICommand<Result<Guid>>;

    public class DeactivatePointAccessHandler(
        IApplicationDbContext context,
        IUserContext userContext) //permet de s'assurer que le user connecté dséactive son point uniquement !
        : ICommandHandler<DeactivatePointAccess, Result<Guid>>
    {
              
        public async ValueTask<Result<Guid>> Handle(
            DeactivatePointAccess command,
            CancellationToken cancellationToken)
        {

            var currentUserId = userContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<Guid>.Unauthorized();

           

            var pointAccess = await context.PointAccesses
                .FirstOrDefaultAsync(pa => pa.Id == command.Id, cancellationToken);

            if (pointAccess is null)
                return Result<Guid>.NotFound("Point d'accès introuvable.");
            
            // Sécurité : un utilisateur standard ne peut désactiver que son propre point d'accès.
            // L'administrateur peut intervenir sur tous les points d'accès.

            if (pointAccess.UserId != currentUserId && !userContext.IsInRole("Administrateur"))
                return Result<Guid>.Forbidden();


            // TODO : ajouter règle métier → pas de désactivation si partage actif

            var result = pointAccess.Desactiver();

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            return Result.Success(pointAccess.Id);
        }
    }
}