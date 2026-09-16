using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSessions.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    internal sealed class BrowseServerHandler : IGenericEventHandler<OpcCommandEvent>
    {
        public OpcCommandEvent EventType => OpcCommandEvent.Browse;

        /// <summary>
        /// Logger for reporting errors and diagnostic information.
        /// </summary>
        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// OPC UA sessions manager.
        /// </summary>
        private readonly IOpcSessionsManager _sessions;

        /// <summary>
        /// Initializes a new instance of <see cref="BrowseServerHandler"/>.
        /// </summary>
        /// <param name="logger">Event dispatcher logger.</param>
        /// <param name="sessions">OPC UA sessions manager.</param>
        public BrowseServerHandler(IGenericEventDispatcherLogger logger, IOpcSessionsManager sessions)
        {
            _logger = logger;
            _sessions = sessions;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the <see cref="BrowseServer"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Validates the event type, checks for session registration, browses child nodes
        /// and passes each of them to the node callback.
        /// Logs appropriate errors if the session does not exist or event type is unsupported.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {
            if (!(@event is BrowseServer browseCommand))
            {
                _logger.LogError($"BrowseServerHandler: incoming event unsupportable" +
                                 $" {@event.GetType().FullName}");
                return;
            }

            var session = _sessions.GetSession(browseCommand.AppId);
            if (session == null)
            {
                _logger.LogError($"BrowseServerHandler: Session for app " +
                                 $"{browseCommand.AppId} not registered. create session before browse server structure");
                return;
            }

            var nodes = await OpcBrowser.BrowseAsync(session, browseCommand.NodeFullName, token);
            foreach (var node in nodes)
                browseCommand.NodeAction(node);
        }
    }
}
