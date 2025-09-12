namespace OpcConfigFactory.Abstract
{
    public interface ITransportParameters
    {
        /// <summary>
        /// The lifetime of a secure channel (in seconds).
        /// </summary>
        /// <value>The channel lifetime.</value>
        int ChannelLifeTimeSec { get; }

        /// <summary>
        /// Sets how frequently the server is pinged to see if communication is still working.
        /// </summary>
        /// <remarks>
        /// This interval controls how much time elaspes before a communication error is detected.
        /// If everything is ok the KeepAlive event will be raised each time this period elapses.
        /// </remarks>
        int KeepAliveIntervalSec { get; }

        /// <summary>
        /// The maximum size of the buffer to use when sending messages.
        /// </summary>
        /// <value>The max size of the buffer.</value>
        int MaxBufferSizeBytes { get; }

        /// <summary>
        /// The maximum length of a message body.
        /// </summary>
        /// <value>The max size of the message.</value>
        int MaxMessageSizeBytes { get; }

        /// <summary>
        /// The minimum lifetime for a subscription (in seconds).
        /// </summary>
        /// <value>The minimum lifetime for a subscription.</value>
        int MinSubscriptionLifeTimeSec { get; }

        /// <summary>
        /// The default timeout to use when sending requests (in seconds).
        /// </summary>
        /// <value>The operation timeout.</value>
        int OperationTimeoutSec { get; }

        /// <summary>
        /// The lifetime of a security token (in seconds).
        /// </summary>
        /// <value>The security token lifetime.</value>
        int SecurityTokenLifeTimeSec { get; }

        /// <summary>
        /// The default session timeout (in seconds).
        /// </summary>
        /// <value>The default session timeout.</value>
        int SessionTimeoutSec { get; }
    }
}