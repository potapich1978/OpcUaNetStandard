using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSessions.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    /// <summary>
    /// Handles <see cref="RegisterSession"/> events.
    /// </summary>
    /// <remarks>
    /// Registers a new OPC UA session using the <see cref="IOpcSessionsManager"/> manager.
    /// Logs errors if the event type is unsupported.
    /// </remarks>
    internal sealed class RegisterSessionHandler : IGenericEventHandler<OpcCommandEvent>
    {
        private sealed class SessionParam : ISessionChannelParams
        {
            /// <inheritdoc/>
            public string AppId { get; internal set; }

            /// <inheritdoc/>
            public string ServerEndPoint { get; internal set; }

            /// <inheritdoc/>
            public int ChannelLifeTimeSec { get; internal set; } = 300;

            /// <inheritdoc/>
            public int KeepAliveIntervalSec { get; internal set; } = 10;

            /// <inheritdoc/>
            public int MaxBufferSizeBytes { get; internal set; } = 64 * 1024;

            /// <inheritdoc/>
            public int MaxMessageSizeBytes { get; internal set; } = 4 * 1024 * 1024;

            /// <inheritdoc/>
            public int MinSubscriptionLifeTimeSec { get; internal set; } = 10;

            /// <inheritdoc/>
            public int OperationTimeoutSec { get; internal set; } = 5;

            /// <inheritdoc/>
            public int SecurityTokenLifeTimeSec { get; internal set; } = 600;

            /// <inheritdoc/>
            public int SessionTimeoutSec { get; internal set; } = 60;
        }


        /// <summary>
        /// OPC UA sessions manager.
        /// </summary>
        private readonly IOpcSessionsManager _sessions;

        /// <summary>
        /// Logger for reporting errors and diagnostics.
        /// </summary>
        private readonly IGenericEventDispatcherLogger _logger;

        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.RegisterSession;

        /// <summary>
        /// Initializes a new instance of <see cref="RegisterSession"/>.
        /// </summary>
        /// <param name="sessions">OPC UA sessions manager.</param>
        /// <param name="logger">Event dispatcher logger.</param>
        public RegisterSessionHandler(IOpcSessionsManager sessions, IGenericEventDispatcherLogger logger)
        {
            _sessions = sessions;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the <see cref="Events.RegisterSession"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Adds a new session using the sessions manager. Logs error if the event type is unsupported.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {
            if (!(@event is RegisterSession registerCommand))
            {
                return Task.Run(() =>
                {
                    _logger.LogError($"RegisterSession: incoming event unsupportable {@event.GetType().FullName}");
                });
            }
            return _sessions.AddSession(new SessionParam {
                AppId = registerCommand.AppId,
                ServerEndPoint = registerCommand.Endpoint,
                ChannelLifeTimeSec = registerCommand.ChannelLifeTimeSec,
                KeepAliveIntervalSec = registerCommand.KeepAliveIntervalSec,
                MaxBufferSizeBytes = registerCommand.MaxBufferSizeBytes,
                MaxMessageSizeBytes = registerCommand.MaxMessageSizeBytes,
                MinSubscriptionLifeTimeSec = registerCommand.MinSubscriptionLifeTimeSec,
                SecurityTokenLifeTimeSec = registerCommand.SecurityTokenLifeTimeSec,
                OperationTimeoutSec = registerCommand.OperationTimeoutSec,
                SessionTimeoutSec = registerCommand.SessionTimeoutSec
            });
        }
    }
}
