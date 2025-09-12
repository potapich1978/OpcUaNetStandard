using EventLogger;
using Opc.Ua.Client;
using OpcSessionFactory.Abstract;
using OpcSessions.Abstract;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OpcSessions
{
    /// <remarks>
    /// Manages the lifecycle of multiple OPC UA sessions. 
    /// Provides functionality to create, retrieve, and dispose sessions 
    /// mapped by application identifiers.
    /// </remarks>
    internal sealed class OpcSessionsManager : IOpcSessionsManager
    {
        private sealed class SessionParams : ISessionParams
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

            /// <inheritdoc/>
            public CancellationToken  CancellationToken { get; internal set; }
        }


        /// <summary>
        /// Stores active OPC UA sessions keyed by application identifier.
        /// </summary>
        /// <remarks>
        /// Ensures concurrent access safety across multiple threads.
        /// </remarks>
        private readonly ConcurrentDictionary<string, IOpcUaSession> sessions_;

        /// <summary>
        /// Logger used for reporting warnings and diagnostic information.
        /// </summary>
        private readonly IGenericEventDispatcherLogger logger_;

        /// <summary>
        /// Factory responsible for creating new OPC UA session instances.
        /// </summary>
        private readonly IOpcSessionFactory factory_;

        /// <remarks>
        /// Initializes a new instance of the sessions manager 
        /// with dependencies for session creation and logging.
        /// </remarks>
        public OpcSessionsManager(IOpcSessionFactory factory, IGenericEventDispatcherLogger logger)
        {
            logger_ = logger;
            factory_ = factory;
            sessions_ = new ConcurrentDictionary<string, IOpcUaSession>();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Adds a new OPC UA session for the given application identifier and server URL. 
        /// If a session with the same ID already exists, a warning is logged 
        /// and the operation is skipped.
        /// </remarks>
        public async Task AddSession(ISessionChannelParams sessionParams)
        {
            var appId = sessionParams.AppId;
            var url = sessionParams.ServerEndPoint;

            if (sessions_.TryGetValue(appId, out var session))
            {
                logger_.LogWarning($"session for app {appId} already exist");
                await Task.CompletedTask;
            }

            var newSession = await factory_.GetSession(new SessionParams { 
                AppId=appId, 
                ServerEndPoint=url,
                ChannelLifeTimeSec=sessionParams.ChannelLifeTimeSec,
                KeepAliveIntervalSec=sessionParams.KeepAliveIntervalSec,
                MaxBufferSizeBytes=sessionParams.MaxBufferSizeBytes,
                MaxMessageSizeBytes=sessionParams.MaxMessageSizeBytes,
                MinSubscriptionLifeTimeSec=sessionParams.MinSubscriptionLifeTimeSec,
                OperationTimeoutSec=sessionParams.OperationTimeoutSec,
                SecurityTokenLifeTimeSec=sessionParams.SecurityTokenLifeTimeSec,
                SessionTimeoutSec = sessionParams.SessionTimeoutSec,
                CancellationToken = sessionParams.CancellationToken
            });

            sessions_.AddOrUpdate(appId, newSession, (k, s) => newSession);
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Retrieves the underlying OPC UA session object for the given application identifier. 
        /// Returns <c>null</c> if the session does not exist.
        /// </remarks>
        public ISession GetSession(string appId)
        {
            if(sessions_.TryGetValue(appId,out var session))
                return session.Session;

            return null;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Removes an OPC UA session by application identifier 
        /// and ensures that resources are disposed properly.
        /// </remarks>
        public void RemoveSession(string appId)
        {
            if(sessions_.TryRemove(appId, out var session))
                session.Dispose();
        }
    }
}
