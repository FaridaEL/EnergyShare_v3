using EnergyShare_v3.Infrastructure.Helpers;
using Mediator;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace EnergyShare_v3.Infrastructure.Behaviors
{       /*Note : Le TransactionBehavior n'encapsule dans une transaction que les Commands. 
         * Les Queries (lectures) n'ont pas besoin de transaction et passent directement au behavior suivant.*/
    public class TransactionBehavior<TMessage, TResponse>
     : IPipelineBehavior<TMessage, TResponse>
     where TMessage : IMessage
    {
        static readonly TransactionOptions s_transactionOptions = new()
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.MaximumTimeout
        };

        public async ValueTask<TResponse> Handle(
            TMessage message,
            MessageHandlerDelegate<TMessage, TResponse> next,
            CancellationToken cancellationToken)
        {
            // Transactions uniquement pour les Commands
            if (message.GetType().IsCommand())
            {
                using var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    s_transactionOptions,
                    TransactionScopeAsyncFlowOption.Enabled);

                var response = await next(message, cancellationToken);
                scope.Complete();
                return response;
            }
            else
            {
                // Les Queries passent directement
                return await next(message, cancellationToken);
            }
        }
    }
}
