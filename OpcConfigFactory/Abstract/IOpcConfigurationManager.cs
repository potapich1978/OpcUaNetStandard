using Opc.Ua;
using System.Threading.Tasks;

namespace OpcConfigFactory.Abstract
{
    /// <summary>
    /// Provides functionality for producing OPC UA client application configurations.
    /// </summary>
    /// <remarks>
    /// Implementations are expected to build a valid <see cref="ApplicationConfiguration"/>
    /// object for OPC UA applications, including security and transport settings.
    /// </remarks>
    public interface IOpcConfigurationManager
    {
        /// <summary>
        /// Produces and returns an OPC UA application configuration for the specified application name.
        /// </summary>
        /// <param name="appName">The name of the OPC UA application or client</param>
        /// <returns>An <see cref="ApplicationConfiguration"/> instance configured for the application.</returns>
        /// <remarks>
        /// Implementations may use caching to avoid re-creating configurations for the same application name.
        /// </remarks>
        Task<ApplicationConfiguration> ProduceConfig(string appName, ITransportParameters transportParameters);
    }
}
