using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Infrastructure.Helpers;
using Mediator;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Behaviors
{       /* Attention : C'est pour cela que les handlers de Command ne doivent pas appeler SaveChangesAsync() eux-memes.
         * Le UnitOfWorkBehavior s'en charge apres le handler, dans la transaction.*/
    public class UnitOfWorkBehavior<TMessage, TResponse>(
    IApplicationDbContext context)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    {
        public async ValueTask<TResponse> Handle(
            TMessage message,
            MessageHandlerDelegate<TMessage, TResponse> next,
            CancellationToken cancellationToken)
        {
            // Appelle le handler d'abord
            var response = await next(message, cancellationToken);

            // SaveChanges uniquement pour les Commands
            if (message.GetType().IsCommand())
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            return response;
        }
    }
}
