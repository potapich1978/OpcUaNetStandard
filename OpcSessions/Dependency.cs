using Microsoft.Extensions.DependencyInjection;
using OpcSessionFactory;
using OpcSessions.Abstract;

namespace OpcSessions
{
    /// <remarks>
    /// Provides dependency injection extensions for registering the OPC session manager 
    /// and its related factories into the service collection.
    /// </remarks>
    public static class Dependency
    {
        /// <remarks>
        /// Registers the OPC session manager as a singleton service, 
        /// ensuring that the session factory is also available.
        /// </remarks>
        public static IServiceCollection AddOpcSessionManager(this IServiceCollection services)
        {
            return services
                .AddOpcSessionFactory()
                .AddSingleton<IOpcSessionsManager, OpcSessionsManager>();
        }
    }
}
