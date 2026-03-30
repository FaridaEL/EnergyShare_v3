using Mediator;
using Microsoft.VisualBasic;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using System.Diagnostics;

namespace EnergyShare_v3.Infrastructure.Behaviors
{      /*
        Pipeline Behaviors et 1 helper dans Infrastructure/. L'ordre est important :
            1. LoggingBehavior      → Mesure le temps total (inclut validation + transaction)
            2. ValidationBehavior   → Rejette les requetes invalides AVANT la transaction
            3. TransactionBehavior  → Encapsule les Commands dans une transaction
            4. UnitOfWorkBehavior   → SaveChanges apres le handler (dans la transaction)  
        */
    public class LoggingBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    {
        public async ValueTask<TResponse> Handle(
            TMessage message,
            MessageHandlerDelegate<TMessage, TResponse> next,
            CancellationToken cancellationToken)
        {
            var log = Log.ForContext(
                "SourceContext",
                //Serilog.Core.Constants.SourceContextPropertyName,
                message.GetType().FullName);

            using (LogContext.PushProperty("RequestName", message.GetType().Name))
            {
                var start = Stopwatch.GetTimestamp();
                TResponse response;

                try
                {
                    response = await next(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    var errorElapsed = GetElapsedMilliseconds(
                        start, Stopwatch.GetTimestamp());
                    log.Error(ex,
                        "Request {RequestName} FAILED in {Elapsed:0.0000} ms",
                        message.GetType().Name, errorElapsed);
                    throw;
                }

                var elapsedMs = GetElapsedMilliseconds(
                    start, Stopwatch.GetTimestamp());
                log.Information(
                    "Request {RequestName} completed in {Elapsed:0.0000} ms",
                    message.GetType().Name, elapsedMs);

                return response;
            }
        }

        static double GetElapsedMilliseconds(long start, long stop)
        {
            var elapsed = (stop - start) * 1000 / (double)Stopwatch.Frequency;
            return Math.Round(elapsed, 2);
        }
    }
}
