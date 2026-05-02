using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Messages
{
    public record GetInboxMessages()
        : IQuery<Result<List<MessageDto>>>;

    public class GetInboxMessagesHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<GetInboxMessages, Result<List<MessageDto>>>
    {
        public async ValueTask<Result<List<MessageDto>>> Handle(
            GetInboxMessages query,
            CancellationToken cancellationToken)
        {
            var currentUserId = userContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<List<MessageDto>>.Unauthorized();

            var messages = await context.Messages
                .AsNoTracking()
                .Where(m => m.DestinataireId == currentUserId.Value)
                .OrderByDescending(m => m.DateEnvoi)
                .Select(m => new MessageDto(
                    m.Id,
                    m.ObjetMessage,
                    m.Contenu,
                    m.DateEnvoi,
                    m.IsLu,
                    m.ExpediteurId,
                    //m.Expediteur.Email,  //on n'expose par l'email  --> mais le prénom plutôt 
                    m.Expediteur.FirstName ?? "Utilisateur",
                    m.DestinataireId,
                    //m.Destinataire.Email,
                    m.Destinataire.FirstName ?? "Utilisateur",
                    m.MatchId
                ))
                .ToListAsync(cancellationToken);

            return Result.Success(messages);
        }
    }
}