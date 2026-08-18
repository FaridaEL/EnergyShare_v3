using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Enums;
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

            // 1. Vérifie que l'utilisateur est bien connecté.
            var currentUserId = userContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<Guid>.Unauthorized();

            // 2. Recherche le point d'accès à désactiver.
            var pointAccess = await context.PointAccesses
                .FirstOrDefaultAsync(pa => pa.Id == command.Id, cancellationToken);

            if (pointAccess is null)
                return Result<Guid>.NotFound("Point d'accès introuvable.");
            
            // 3. Sécurité : un utilisateur standard ne peut désactiver que son propre point d'accès.
            // L'administrateur peut intervenir sur tous les points d'accès.

            if (pointAccess.UserId != currentUserId && !userContext.IsInRole("Administrateur"))
                return Result<Guid>.Forbidden();


            // 4.RÈGLE MÉTIER :  Un point d'accès ne peut pas être désactivé tant qu'il participe
            // encore à un partage d'énergie qui n'est pas clôturé.
            //
            // ExitAt == null signifie que le point est toujours membre du partage.
            // Un partage clôturé ne bloque plus la désactivation du point.
            var participeAUnPartageNonCloture = await context.MembresPartage
                .AsNoTracking()
                .AnyAsync(
                    participation =>
                        participation.PointAccessId == pointAccess.Id
                        && participation.ExitAt == null
                        && participation.Partage.Statut != PartageEnergieStatutType.Cloture,
                    cancellationToken);

            //5. Si le point participe à un partage non clôturé, on retourne une erreur et désactivation est refusée.
            if (participeAUnPartageNonCloture)
            {
                return Result<Guid>.Error(
                    "Ce point participe encore à un partage d'énergie. " +
                    "Quittez ou clôturez d'abord ce partage avant de désactiver le point.");
            }

            //6. Désactivation du point d'accès car règle métier est respectée. 
            var result = pointAccess.Desactiver();

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            return Result.Success(pointAccess.Id);
        }
    }
}