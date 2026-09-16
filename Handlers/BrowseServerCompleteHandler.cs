using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSessions.Abstract;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    internal sealed class BrowseServerCompleteHandler : IGenericEventHandler<OpcCommandEvent>
    {
        public OpcCommandEvent EventType => OpcCommandEvent.BrowseComplete;

        /// <summary>
        /// Logger for reporting errors and diagnostic information.
        /// </summary>
        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// OPC UA sessions manager.
        /// </summary>
        private readonly IOpcSessionsManager _sessions;

        /// <summary>
        /// Initializes a new instance of <see cref="BrowseServerCompleteHandler"/>.
        /// </summary>
        /// <param name="logger">Event dispatcher logger.</param>
        /// <param name="sessions">OPC UA sessions manager.</param>
        public BrowseServerCompleteHandler(IGenericEventDispatcherLogger logger, IOpcSessionsManager sessions)
        {
            _logger = logger;
            _sessions = sessions;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the <see cref="BrowseServerComplete"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Validates the event type, checks for session registration, browses child nodes
        /// and delivers the whole level to the completion callback once.
        /// A missing session or a browse failure is delivered to the callback as the error.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {
            if (!(@event is BrowseServerComplete browseCommand))
            {
                _logger.LogError($"BrowseServerCompleteHandler: incoming event unsupportable" +
                                 $" {@event.GetType().FullName}");
                return;
            }

            var session = _sessions.GetSession(browseCommand.AppId);
            if (session == null)
            {
                var message = $"BrowseServerCompleteHandler: Session for app " +
                              $"{browseCommand.AppId} not registered. create session before browse server structure";
                _logger.LogError(message);
                browseCommand.Completed(null, new InvalidOperationException(message));
                return;
            }

            List<OpcNodeInfo> nodes;
            try
            {
                nodes = await OpcBrowser.BrowseAsync(session, browseCommand.NodeFullName, token);
            }
            catch (Exception error)
            {
                browseCommand.Completed(null, error);
                throw;
            }

            browseCommand.Completed(nodes, null);
        }
    }
}
