using Microsoft.Extensions.DependencyInjection;
using OpcConfigFactory;
using OpcSessionFactory.Abstract;

namespace OpcSessionFactory
{
    /// <remarks>
    /// Provides dependency injection extensions for registering the OPC session factory 
    /// and its configuration dependencies within an IoC container.
    /// </remarks>
    public static class Dependency
    {
        /// <remarks>
        /// Registers the OPC session factory as a singleton service 
        /// and ensures the OPC configuration manager is available.
        /// </remarks>
        public static IServiceCollection AddOpcSessionFactory(this IServiceCollection services)
        {
            return services
                .AddOpcConfigManager()
                .AddSingleton<IOpcFoundationSession, OpcFoundationSession>()
                .AddSingleton<IOpcSessionFactory, OpcSessionFactory>();
        }
    }
}
