using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSessions.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    internal sealed class WriteTagValueCompleteHandler : IGenericEventHandler<OpcCommandEvent>
    {
        public OpcCommandEvent EventType => OpcCommandEvent.WriteTagValueComplete;

        /// <summary>
        /// Logger for reporting errors and diagnostic information.
        /// </summary>
        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// OPC UA sessions manager.
        /// </summary>
        private readonly IOpcSessionsManager _sessions;

        /// <summary>
        /// Initializes a new instance of <see cref="WriteTagValueCompleteHandler"/>.
        /// </summary>
        /// <param name="logger">Event dispatcher logger.</param>
        /// <param name="sessions">OPC UA sessions manager.</param>
        public WriteTagValueCompleteHandler(IGenericEventDispatcherLogger logger, IOpcSessionsManager sessions)
        {
            _logger = logger;
            _sessions = sessions;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the <see cref="WriteTagValueComplete"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Validates the event type, checks for session registration, writes the value
        /// and reports the outcome to the completion callback once.
        /// A missing session or a write failure is delivered to the callback as the error.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {
            if (!(@event is WriteTagValueComplete writeCommand))
            {
                _logger.LogError($"WriteTagValueCompleteHandler: incoming event unsupportable" +
                                 $" {@event.GetType().FullName}");
                return;
            }

            var session = _sessions.GetSession(writeCommand.AppId);
            if (session == null)
            {
                var message = $"WriteTagValueCompleteHandler: Session for app " +
                              $"{writeCommand.AppId} not registered. create session before write tag value";
                _logger.LogError(message);
                writeCommand.Completed(new InvalidOperationException(message));
                return;
            }

            try
            {
                await OpcWriter.WriteAsync(session, writeCommand.TagId, writeCommand.Value, token);
            }
            catch (Exception error)
            {
                writeCommand.Completed(error);
                throw;
            }

            writeCommand.Completed(null);
        }
    }
}
