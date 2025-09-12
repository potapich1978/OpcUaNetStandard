using EventLogger;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OpcSessionFactory.Abstract;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpcSessionFactory
{
    internal sealed class OpcFoundationSession : IOpcFoundationSession
    {

        private readonly IGenericEventDispatcherLogger _logger;

        public OpcFoundationSession(IGenericEventDispatcherLogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ISession> ProduceBaseOpcSession(ApplicationConfiguration configuration, ConfiguredEndpoint endpoint, 
            bool updateBeforeConnect, string sessionName, uint sessionTimeout, IUserIdentity identity, 
            IList<string> preferredLocales)
        {
            return await Session.Create(
                configuration,
                endpoint,
                updateBeforeConnect: false,
                sessionName: sessionName,
                sessionTimeout: sessionTimeout,
                identity: identity,
                preferredLocales: null
            ).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<EndpointDescription> SelectEndpoint(ApplicationConfiguration configuration, string endpoint, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                EndpointDescription endpointDescription = null;

                while (endpointDescription == null)
                {
                    if(cancellationToken.IsCancellationRequested)
                        break;
                    try
                    {
                        endpointDescription = CoreClientUtils.SelectEndpoint(configuration, endpoint, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"RegisterSessionHandler {DateTime.Now:HH:mm:ss}: {ex.Message}", ex);
                        Thread.Sleep(5000);
                    }
                }

                return endpointDescription;
            });

        }

        /// <inheritdoc/>
        public Task<bool> ValidatAppInstance(ApplicationInstance appInstance)
            => appInstance.CheckApplicationInstanceCertificates(true);
    }
}
