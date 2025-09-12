using Microsoft.Extensions.DependencyInjection;
using OpcCallbacks.Abstract;

namespace OpcCallbacks
{
    /// <summary>
    /// Provides extension methods for configuring dependency injection of OPC callback management services.
    /// </summary>
    public static class Dependency
    {
        /// <summary>
        /// Registers the OPC callback manager and related services with the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <returns>The service collection with OPC callback management services registered.</returns>
        /// <remarks>
        /// This method registers:
        /// <list type="bullet">
        /// <item><description><see cref="IOpcTagCallbacksCount"/> implemented by <see cref="OpcTagCallbacksCounter"/></description></item>
        /// <item><description><see cref="IOpcCallbackManager"/> implemented by <see cref="OpcCallbacksManager"/></description></item>
        /// </list>
        /// Both services are registered as singletons to maintain consistent state across the application.
        /// </remarks>
        public static IServiceCollection AddCallbackManager(this IServiceCollection services)
        {
            return services
                .AddSingleton<IOpcTagCallbacksCount, OpcTagCallbacksCounter>()
                .AddSingleton<IOpcCallbackManager, OpcCallbacksManager>();
        }
    }
}
