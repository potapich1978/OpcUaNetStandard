using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSessions.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    internal sealed class WriteTagValueHandler : IGenericEventHandler<OpcCommandEvent>
    {
        public OpcCommandEvent EventType => OpcCommandEvent.WriteTagValue;

        /// <summary>
        /// Logger for reporting errors and diagnostic information.
        /// </summary>
        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// OPC UA sessions manager.
        /// </summary>
        private readonly IOpcSessionsManager _sessions;

        /// <summary>
        /// Initializes a new instance of <see cref="WriteTagValueHandler"/>.
        /// </summary>
        /// <param name="logger">Event dispatcher logger.</param>
        /// <param name="sessions">OPC UA sessions manager.</param>
        public WriteTagValueHandler(IGenericEventDispatcherLogger logger, IOpcSessionsManager sessions)
        {
            _logger = logger;
            _sessions = sessions;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the <see cref="WriteTagValue"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Validates the event type, checks for session registration and writes the value.
        /// Logs appropriate errors if the session does not exist or event type is unsupported.
        /// A write failure is left to the dispatcher, which logs it.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {
            if (!(@event is WriteTagValue writeCommand))
            {
                _logger.LogError($"WriteTagValueHandler: incoming event unsupportable" +
                                 $" {@event.GetType().FullName}");
                return;
            }

            var session = _sessions.GetSession(writeCommand.AppId);
            if (session == null)
            {
                _logger.LogError($"WriteTagValueHandler: Session for app " +
                                 $"{writeCommand.AppId} not registered. create session before write tag value");
                return;
            }

            await OpcWriter.WriteAsync(session, writeCommand.TagId, writeCommand.Value, token);
        }
    }
}
