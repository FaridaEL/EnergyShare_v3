using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Messages
{
    public record GetSentMessages()
        : IQuery<Result<List<MessageDto>>>;

    public class GetSentMessagesHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : IQueryHandler<GetSentMessages, Result<List<MessageDto>>>
    {
        public async ValueTask<Result<List<MessageDto>>> Handle(
            GetSentMessages query,
            CancellationToken cancellationToken)
        {
            var currentUserId = userContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<List<MessageDto>>.Unauthorized();

            var messages = await context.Messages
                .AsNoTracking()
                .Where(m => m.ExpediteurId == currentUserId.Value)
                .OrderByDescending(m => m.DateEnvoi)
                .Select(m => new MessageDto(
                    m.Id,
                    m.ObjetMessage,
                    m.Contenu,
                    m.DateEnvoi,
                    m.IsLu,
                    m.ExpediteurId,
                    m.Expediteur.Email,
                    m.DestinataireId,
                    m.Destinataire.Email,
                    m.MatchId
                ))
                .ToListAsync(cancellationToken);

            return Result.Success(messages);
        }
    }
}