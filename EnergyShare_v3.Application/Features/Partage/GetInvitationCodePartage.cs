using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{
    // Command appelée lorsque le créateur clique sur "Inviter un membre".
    // Elle ne crée pas une participation.
    // Elle garantit seulement qu’un code d’invitation valide existe pour le partage.
    public record GetInvitationCodePartage(Guid PartageId)
        : ICommand<Result<InvitationCodeDto>>;

    public class GetInvitationCodePartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<GetInvitationCodePartage, Result<InvitationCodeDto>>
    {
        public async ValueTask<Result<InvitationCodeDto>> Handle(
            GetInvitationCodePartage command,
            CancellationToken cancellationToken)
        {
            // Sécurité : seul un utilisateur connecté peut inviter quelqu’un.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<InvitationCodeDto>.Unauthorized();

            var currentUserId = userContext.UserId.Value;

            // On récupère le partage concerné.
            var partage = await context.Partages
                .FirstOrDefaultAsync(p => p.Id == command.PartageId, cancellationToken);

            if (partage is null)
                return Result<InvitationCodeDto>.NotFound("Partage introuvable.");

            // Sécurité métier :
            // seul le vendeur/créateur du partage peut inviter un membre.
            // L’administrateur est autorisé à titre exceptionnel.
            if (partage.VendeurId != currentUserId &&
                !userContext.IsInRole("Administrateur"))
                return Result<InvitationCodeDto>.Forbidden();

            // Méthode métier du domaine :
            // - si aucun code n’existe, elle en crée un ;
            // - si le code est expiré, elle en crée un nouveau ;
            // - si le code est encore valide, elle garde le code existant.
            var result = partage.EnsureValidInvitationCode();

            if (!result.IsSuccess)
                return Result<InvitationCodeDto>.Invalid(result.ValidationErrors);

            // Pas de SaveChangesAsync -> persistance gérée par le UnitOfWorkBehavior.

            return Result.Success(new InvitationCodeDto(
                partage.Id,
                partage.InvitationCode!,
                partage.InvitationCodeExpiresAt!.Value
            ));
        }
    }
}