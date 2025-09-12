using Microsoft.Extensions.DependencyInjection;
using OpcConfigFactory.Abstract;

namespace OpcConfigFactory
{
    /// <summary>
    /// Provides extension methods for registering OPC UA configuration manager services
    /// in a dependency injection container.
    /// </summary>
    /// <remarks>
    /// This class integrates <see cref="IOpcConfigurationManager"/> into the 
    /// Microsoft.Extensions.DependencyInjection pipeline for convenient use in .NET applications.
    /// </remarks>
    public static class Dependency
    {
        /// <summary>
        /// Registers <see cref="IOpcConfigurationManager"/> and its implementation
        /// (<see cref="OpcConfigurationManager"/>) as a singleton service.
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        /// <returns>The updated service collection.</returns>
        /// <remarks>
        /// After registration, any component resolved via dependency injection can request
        /// <see cref="IOpcConfigurationManager"/> to obtain OPC UA configurations.
        /// </remarks>
        public static IServiceCollection AddOpcConfigManager(this IServiceCollection services)
        {
            return services.AddSingleton<IOpcConfigurationManager, OpcConfigurationManager>();
        }
    }
}
