using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSessions.Abstract;
using OpcSubscriptions.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    /// <summary>
    /// Handles <see cref="AddItemToSubscription"/> events.
    /// </summary>
    /// <remarks>
    /// Adds a monitored item to an existing OPC UA subscription.
    /// Logs errors if the session is not registered or the event type is unsupported.
    /// </remarks>
    internal sealed class AddItemToSubscriptionHandler : IGenericEventHandler<OpcCommandEvent>
    {
        /// <summary>
        /// Logger for reporting errors and diagnostic information.
        /// </summary>
        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// OPC UA subscription manager.
        /// </summary>
        private readonly IOpcSubscriptions _subscriptions;

        /// <summary>
        /// OPC UA sessions manager.
        /// </summary>
        private readonly IOpcSessionsManager _sessions;

        // <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.AddItemToSubscription;

        /// <summary>
        /// Initializes a new instance of <see cref="AddItemToSubscriptionHandler"/>.
        /// </summary>
        /// <param name="logger">Event dispatcher logger.</param>
        /// <param name="sessions">OPC UA sessions manager.</param>
        /// <param name="subscriptions">OPC UA subscriptions manager.</param>
        public AddItemToSubscriptionHandler(IGenericEventDispatcherLogger logger, IOpcSessionsManager sessions, 
                                            IOpcSubscriptions subscriptions)
        {
            _logger = logger;
            _sessions = sessions;
            _subscriptions = subscriptions;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the <see cref="AddItemToSubscription"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Validates the event type, checks for session registration, and adds the item to subscription.
        /// Logs appropriate errors if the session does not exist or event type is unsupported.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {
            if (!(@event is AddItemToSubscription addCommand))
                _logger.LogError($"AddItemToSubscriptionHandler: incoming event unsupportable" +
                                 $" {@event.GetType().FullName}");

            else
            {
                var session = _sessions.GetSession(addCommand.AppId);
                if (session == null)
                {
                    _logger.LogError($"AddItemToSubscriptionHandler: Session for app " +
                                     $"{addCommand.AppId} not registered. create session before add subscription");
                }
                else
                {
                    await _subscriptions.AddSubscriprion(
                        session,
                        addCommand.AppId,
                        addCommand.TagId,
                        addCommand.EventKey,
                        addCommand.Callback);
                }
            }
            await Task.CompletedTask;
        }
    }
}
