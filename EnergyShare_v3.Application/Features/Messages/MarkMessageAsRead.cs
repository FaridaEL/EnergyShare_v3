using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Messages
{
    public record MarkMessageAsRead(Guid Id)
        : ICommand<Result<Guid>>;

    public class MarkMessageAsReadHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<MarkMessageAsRead, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            MarkMessageAsRead command,
            CancellationToken cancellationToken)
        {
            var currentUserId = userContext.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<Guid>.Unauthorized();

            var entity = await context.Messages
                .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

            if (entity is null)
                return Result<Guid>.NotFound("Message introuvable.");

            if (entity.DestinataireId != currentUserId.Value)
                return Result<Guid>.Forbidden();

            entity.MarquerCommeLu();

            return Result.Success(entity.Id);
        }
    }
}
