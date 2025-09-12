using EventLogger;
using Opc.Ua;
using Opc.Ua.Configuration;
using OpcConfigFactory.Abstract;
using OpcSessionFactory.Abstract;
using System.Threading.Tasks;

namespace OpcSessionFactory
{
    /// <remarks>
    /// Factory responsible for creating and managing OPC UA sessions. 
    /// It handles configuration, certificate validation, endpoint selection, 
    /// and establishing secure connections to OPC UA servers.
    /// </remarks>
    internal class OpcSessionFactory : IOpcSessionFactory
    {
        private sealed class TransportParameters : ITransportParameters
        {
            /// <inheritdoc/>
            public int OperationTimeoutSec { get; internal set; } = 5;

            /// <inheritdoc/>
            public int MaxMessageSizeBytes { get; internal set; } = 4 * 1024 * 1024;

            /// <inheritdoc/>
            public int MaxBufferSizeBytes { get; internal set; } = 64 * 1024;

            /// <inheritdoc/>
            public int ChannelLifeTimeSec { get; internal set; } = 300;

            /// <inheritdoc/>
            public int SecurityTokenLifeTimeSec { get; internal set; } = 600;

            /// <inheritdoc/>
            public int SessionTimeoutSec { get; internal set; } = 60;

            /// <inheritdoc/>
            public int MinSubscriptionLifeTimeSec { get; internal set; } = 10;

            /// <inheritdoc/>
            public int KeepAliveIntervalSec { get; internal set; } = 10;
        }

        private readonly IOpcConfigurationManager _configManager;
        private readonly IGenericEventDispatcherLogger _logger;
        private readonly IOpcFoundationSession _opcFoundationSession;

        /// <remarks>
        /// Initializes a new instance of the session factory 
        /// with configuration and logging dependencies.
        /// </remarks>
        public OpcSessionFactory(IOpcConfigurationManager configManager, IOpcFoundationSession opcFoundationSession, IGenericEventDispatcherLogger logger)
        {
            _configManager = configManager;
            _opcFoundationSession = opcFoundationSession;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Creates and initializes an OPC UA session using application configuration, 
        /// endpoint selection, and anonymous authentication. 
        /// Configures timeouts, keep-alive intervals, and reconnection options.
        /// </remarks>
        public async Task<IOpcUaSession> GetSession(ISessionParams sessionParams)
        {
            var config = await _configManager.ProduceConfig(sessionParams.AppId, new TransportParameters
            {
                ChannelLifeTimeSec = sessionParams.ChannelLifeTimeSec,
                SessionTimeoutSec = sessionParams.SessionTimeoutSec,
                OperationTimeoutSec = sessionParams.OperationTimeoutSec,
                SecurityTokenLifeTimeSec = sessionParams.SecurityTokenLifeTimeSec,
                KeepAliveIntervalSec = sessionParams.KeepAliveIntervalSec,
                MaxBufferSizeBytes = sessionParams.MaxBufferSizeBytes,
                MaxMessageSizeBytes = sessionParams.MaxMessageSizeBytes,
                MinSubscriptionLifeTimeSec = sessionParams.MinSubscriptionLifeTimeSec
            });

            var applicationInstance = new ApplicationInstance
            {
                ApplicationName = sessionParams.AppId,
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };

            await _opcFoundationSession.ValidatAppInstance(applicationInstance);

            var selectedEndpoint = await _opcFoundationSession.SelectEndpoint(
                config, 
                sessionParams.ServerEndPoint, 
                sessionParams.CancellationToken
            );

            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endPoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            var userIdentity = new UserIdentity(new AnonymousIdentityToken());

            var session = await _opcFoundationSession.ProduceBaseOpcSession(
                config,
                endPoint,
                updateBeforeConnect: false,
                sessionName: sessionParams.AppId,
                sessionTimeout: (uint)(sessionParams.SessionTimeoutSec * 1000),
                identity: userIdentity,
                preferredLocales: null
            );

            session.DeleteSubscriptionsOnClose = false;
            session.TransferSubscriptionsOnReconnect = true;
            session.KeepAliveInterval = sessionParams.KeepAliveIntervalSec * 1000;
            session.OperationTimeout = sessionParams.OperationTimeoutSec * 1000;

            return new OpcUaSession(session);
        }
    }
}
