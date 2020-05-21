using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mozilla.IoT.WebThing.WebSockets
{
    /// <summary>
    /// Add event subscription. 
    /// </summary>
    public class AddEventSubscription : IWebSocketAction
    {
        /// <inheritdoc/>
        public string Action => "addEventSubscription";
        
        
        /// <inheritdoc/>
        public ValueTask ExecuteAsync(System.Net.WebSockets.WebSocket socket, Thing thing, object data,
            IServiceProvider provider, CancellationToken cancellationToken)
        {
            // var observer = provider.GetRequiredService<ThingObserver>();
            // var logger = provider.GetRequiredService<ILogger<AddEventSubscription>>();
            //
            // foreach (var eventName in data.EnumerateObject().TakeWhile(eventName => !cancellationToken.IsCancellationRequested))
            // {
            //     if (thing.ThingContext.Events.TryGetValue(eventName.Name, out var @events))
            //     {
            //         events.Added += observer.OnEvenAdded;
            //     }
            //     else
            //     {
            //         logger.LogInformation("{eventName} event not found. [Thing: {thing}]", eventName.Name, thing.Name);
            //     }
            // }
            //
            return new ValueTask();
        }
    }
}
