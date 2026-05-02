using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.Messages;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Messages
{
    public record SendMessage(
        Guid DestinataireId,
        string ObjetMessage,
        string Contenu,
        Guid? MatchId
    ) : ICommand<Result<Guid>>;

    public class SendMessageValidator : AbstractValidator<SendMessage>
    {
        public SendMessageValidator()
        {
            RuleFor(x => x.DestinataireId)
                .NotEmpty()
                .WithMessage("Le destinataire est requis.");

            RuleFor(x => x.ObjetMessage)
                .NotEmpty()
                .WithMessage("L'objet du message est requis.");

            RuleFor(x => x.Contenu)
                .NotEmpty()
                .WithMessage("Le contenu du message est requis.");
        }
    }

    public class SendMessageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<SendMessage, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            SendMessage command,
            CancellationToken cancellationToken)
        {
            var expediteurId = userContext.UserId;  //sécurité : on récupère l'ID de l'expéditeur à partir du contexte utilisateur, pas du client

            if (expediteurId is null || expediteurId == Guid.Empty)
                return Result<Guid>.Unauthorized();

            if (expediteurId == command.DestinataireId)
                return Result<Guid>.Conflict("Vous ne pouvez pas vous envoyer un message à vous-même.");

            var destinataireExists = await context.Users
                .AnyAsync(u => u.Id == command.DestinataireId, cancellationToken);

            if (!destinataireExists)
                return Result<Guid>.NotFound("Destinataire introuvable.");

            if (command.MatchId is not null)
            {
                var matchExists = await context.Matches
                    .AnyAsync(m => m.Id == command.MatchId.Value, cancellationToken);

                if (!matchExists)
                    return Result<Guid>.NotFound("Match introuvable.");
            }

            var result = Message.Create(
                command.ObjetMessage,
                command.Contenu,
                expediteurId.Value,
                command.DestinataireId,
                command.MatchId);

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var entity = result.Value;

            await context.Messages.AddAsync(entity, cancellationToken);

            return Result.Success(entity.Id);
        }
    }
}