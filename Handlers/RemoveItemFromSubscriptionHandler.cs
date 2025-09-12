using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSubscriptions.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    /// <summary>
    /// Handles <see cref="Events.RemoveItemFromSubscription"/> events.
    /// </summary>
    /// <remarks>
    /// Removes a monitored item from an OPC UA subscription.
    /// Logs errors if the event type is unsupported.
    /// </remarks>
    internal sealed class RemoveItemFromSubscriptionHandler : IGenericEventHandler<OpcCommandEvent>
    {
        /// <summary>
        /// Logger for reporting errors and diagnostics.
        /// </summary>
        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// OPC UA subscriptions manager.
        /// </summary>
        private readonly IOpcSubscriptions _subscriptions;

        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.RemoveItemFromSubscription;

        /// <summary>
        /// Initializes a new instance of <see cref="RemoveItemFromSubscriptionHandler"/>.
        /// </summary>
        /// <param name="logger">Event dispatcher logger.</param>
        /// <param name="subscriptions">OPC UA subscriptions manager.</param>
        public RemoveItemFromSubscriptionHandler(IGenericEventDispatcherLogger logger, IOpcSubscriptions subscriptions)
        {
            _logger = logger;
            _subscriptions = subscriptions;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the <see cref="Events.RemoveItemFromSubscription"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Removes a monitored item from the subscription. Logs error if the event type is unsupported.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {
            if (!(@event is Events.RemoveItemFromSubscription removeCommand))
                _logger.LogError($"RemoveItemFromSubscription: incoming event unsupportable {@event.GetType().FullName}");

            else
            {
                await _subscriptions.RemoveItemFromSubscription(
                    removeCommand.AppId, 
                    removeCommand.TagId, 
                    removeCommand.EventKey);
            }

            await Task.CompletedTask;
        }
    }
}
