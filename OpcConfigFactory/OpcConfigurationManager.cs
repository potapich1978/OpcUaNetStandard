using Opc.Ua;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using System;
using OpcConfigFactory.Abstract;

namespace OpcConfigFactory
{
    /// <summary>
    /// Provides a thread-safe implementation of <see cref="IOpcConfigurationManager"/>
    /// that creates and caches OPC UA application configurations.
    /// </summary>
    /// <remarks>
    /// This implementation stores generated configurations in memory using a 
    /// <see cref="ConcurrentDictionary{TKey,TValue}"/> and ensures PKI directories
    /// are created on initialization.
    /// </remarks>
    internal sealed class OpcConfigurationManager : IOpcConfigurationManager
    {
        /// <summary>
        /// Root directory where the PKI certificate stores are located.
        /// </summary>
        /// <remarks>
        /// Subdirectories for "own", "trusted", "issuers", and "rejected" certificates
        /// are created automatically during construction.
        /// </remarks>
        private readonly string _pkiRoot = Path.Combine(AppContext.BaseDirectory, "pki");

        /// <summary>
        /// Internal cache of application configurations, keyed by application name.
        /// </summary>
        /// <remarks>
        /// Prevents repeated creation and validation of <see cref="ApplicationConfiguration"/> instances.
        /// </remarks>
        private readonly ConcurrentDictionary<string, ApplicationConfiguration> _configs 
            = new ConcurrentDictionary<string, ApplicationConfiguration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcConfigurationManager"/> class.
        /// Ensures that required PKI subdirectories are created.
        /// </summary>
        /// <remarks>
        /// Directories are created relative to the application base directory in a "pki" folder.
        /// </remarks>
        public OpcConfigurationManager()
        {
            CreateDirectory("own");
            CreateDirectory("trusted");
            CreateDirectory("issuers");
            CreateDirectory("rejected");
        }

        /// <inheritdoc />
        /// <remarks>
        /// The resulting configuration includes:
        /// <list type="bullet">
        /// <item>Application certificate and trust lists</item>
        /// <item>Transport quotas for client communication</item>
        /// <item>Client session defaults</item>
        /// </list>
        /// The configuration is validated before being cached and returned.
        /// </remarks>
        public async Task<ApplicationConfiguration> ProduceConfig(string appName, ITransportParameters transportParameters)
        {
            if (_configs.TryGetValue(appName, out var config))
                return await Task.FromResult(config);

            config = new ApplicationConfiguration()
            {
                ApplicationName = appName,
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = Path.Combine(_pkiRoot, "own"),
                        SubjectName = "CN=OpcUaConsoleClient"
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = Path.Combine(_pkiRoot, "trusted")
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = Path.Combine(_pkiRoot, "issuers")
                    },
                    RejectedCertificateStore = new CertificateStoreIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = Path.Combine(_pkiRoot, "rejected")
                    },
                    AutoAcceptUntrustedCertificates = true
                },
                TransportQuotas = new TransportQuotas
                {
                    OperationTimeout = transportParameters.OperationTimeoutSec * 1000,
                    MaxMessageSize = transportParameters.MaxMessageSizeBytes,
                    MaxBufferSize = transportParameters.MaxBufferSizeBytes,
                    ChannelLifetime = transportParameters.ChannelLifeTimeSec * 1000,
                    SecurityTokenLifetime = transportParameters.SecurityTokenLifeTimeSec * 1000
                },
                ClientConfiguration = new ClientConfiguration
                {
                    DefaultSessionTimeout = transportParameters.SessionTimeoutSec * 1000,
                    MinSubscriptionLifetime = transportParameters.MinSubscriptionLifeTimeSec * 1000
                }
            };

            await config.Validate(ApplicationType.Client).ConfigureAwait(false);
            _configs.AddOrUpdate(appName, config, (a, c) => c);

            return await Task.FromResult(config);
        }

        /// <summary>
        /// Creates a PKI subdirectory with the given name if it does not already exist.
        /// </summary>
        /// <param name="dirName">The name of the subdirectory to create.</param>
        /// <remarks>
        /// Subdirectories are created under the root PKI folder (<see cref="_pkiRoot"/>).
        /// </remarks>
        private void CreateDirectory(string dirName)
        {
            if(!Directory.Exists(dirName)) 
                Directory.CreateDirectory(Path.Combine(_pkiRoot, dirName));
        }
    }
}
