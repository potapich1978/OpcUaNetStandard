using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpcSessionFactory.Abstract
{
    internal interface IOpcFoundationSession
    {
        /// <summary>
        /// Checks for a valid application instance certificate.
        /// </summary>
        /// <param name="appInstance" <see cref="ApplicationInstance">>application instance.</param>
        Task<bool> ValidatAppInstance(ApplicationInstance appInstance);

        /// <summary>
        /// Creates a new communication session with a server by invoking the CreateSession service
        /// </summary>
        /// <param name="configuration" <see cref="ApplicationConfiguration">> The configuration for the client application.</param>
        /// <param name="endpoint" <see cref="ConfiguredEndpoint">>The endpoint for the server.</param>
        /// <param name="updateBeforeConnect">If set to <c>true</c> the discovery endpoint is used to update the endpoint description before connecting.</param>
        /// <param name="sessionName">The name to assign to the session.</param>
        /// <param name="sessionTimeout">The timeout period for the session.</param>
        /// <param name="identity" <see cref="IUserIdentity">>The identity.</param>
        /// <param name="preferredLocales">The user identity to associate with the session.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The new session object</returns>
        Task<ISession> ProduceBaseOpcSession(
            ApplicationConfiguration configuration,
            ConfiguredEndpoint endpoint,
            bool updateBeforeConnect,
            string sessionName,
            uint sessionTimeout,
            IUserIdentity identity,
            IList<string> preferredLocales);

        /// <summary>
        /// Continuously attempts to resolve the best endpoint for the given server address.
        /// Logs errors and retries until a valid endpoint is found.
        /// </summary>
        /// <param name="configuration" <see cref="ApplicationConfiguration">>The configuration for the client application.</param>
        /// <param name="endpoint">The endpoint for the server.</param>
        /// <param name="cancellationToken">Cancallation token for break retries connect to server</param>
        /// <returns <see cref="EndpointDescription"/>></returns>
        Task<EndpointDescription> SelectEndpoint(ApplicationConfiguration configuration, string endpoint, CancellationToken cancellationToken);
    }
}
