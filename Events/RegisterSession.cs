using ChannelReader.Abstract;

namespace Events
{
    /// <summary>
    /// Event for registering a new OPC UA session.
    /// </summary>
    /// <remarks>
    /// Associates an application identifier with a specific server endpoint.
    /// </remarks>
    public sealed class RegisterSession : IGenericEvent<OpcCommandEvent>
    {
        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.RegisterSession;

        /// <summary>
        /// Application identifier for which the session will be created.
        /// </summary>
        public string AppId { get; private set; }

        /// <summary>
        /// Endpoint URL of the OPC UA server.
        /// </summary>
        public string Endpoint { get; private set; }

        /// <summary>
        /// The lifetime of a secure channel (in milliseconds).
        /// </summary>
        /// <value>The channel lifetime.</value>
        public int ChannelLifeTimeSec { get; private set; }

        /// <summary>
        /// Sets how frequently the server is pinged to see if communication is still working.
        /// </summary>
        /// <remarks>
        /// This interval controls how much time elaspes before a communication error is detected.
        /// If everything is ok the KeepAlive event will be raised each time this period elapses.
        /// </remarks>
        public int KeepAliveIntervalSec { get; private set; }

        /// <summary>
        /// The maximum size of the buffer to use when sending messages.
        /// </summary>
        /// <value>The max size of the buffer.</value>
        public int MaxBufferSizeBytes { get; private set; }

        /// <summary>
        /// The maximum length of a message body.
        /// </summary>
        /// <value>The max size of the message.</value>
        public int MaxMessageSizeBytes { get; private set; }

        /// <summary>
        /// The minimum lifetime for a subscription (in seconds).
        /// </summary>
        /// <value>The minimum lifetime for a subscription.</value>
        public int MinSubscriptionLifeTimeSec { get; private set; }

        /// <summary>
        /// The default timeout to use when sending requests (in seconds).
        /// </summary>
        /// <value>The operation timeout.</value>
        public int OperationTimeoutSec { get; private set; }

        /// <summary>
        /// The lifetime of a security token (in seconds).
        /// </summary>
        /// <value>The security token lifetime.</value>
        public int SecurityTokenLifeTimeSec { get; private set; }

        /// <summary>
        /// The default session timeout (in seconds).
        /// </summary>
        /// <value>The default session timeout.</value>
        public int SessionTimeoutSec { get; private set; }

        /// <summary>
        /// Creates a new <see cref="RegisterSession"/> event instance.
        /// </summary>
        /// <param name="appId">Application identifier.</param>
        /// <param name="endpoint">OPC UA server endpoint URL.</param>
        public RegisterSession(
            string appId,
            string endpoint,
            int channelLifeTimeSec = 300,
            int keepAliveIntervalSec = 10,
            int maxBufferSizeBytes = 64 * 1024,
            int maxMessageSizeBytes = 4 * 1024 * 1024,
            int minSubscriptionLifeTimeSec = 10,
            int operationTimeoutSec = 5,
            int securityTokenLifeTimeSec = 600,
            int sessionTimeoutSec = 60)
        {
            AppId = appId;
            Endpoint = endpoint;
            ChannelLifeTimeSec = channelLifeTimeSec;
            KeepAliveIntervalSec = keepAliveIntervalSec;
            MaxBufferSizeBytes = maxBufferSizeBytes;
            MaxMessageSizeBytes = maxMessageSizeBytes;
            MinSubscriptionLifeTimeSec = minSubscriptionLifeTimeSec;
            OperationTimeoutSec = operationTimeoutSec;
            SecurityTokenLifeTimeSec = securityTokenLifeTimeSec;
            SessionTimeoutSec = sessionTimeoutSec;
        }
    }
}
