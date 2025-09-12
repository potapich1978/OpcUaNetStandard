using Microsoft.Extensions.DependencyInjection;
using OpcCallbacks;
using OpcSubscriptions.Abstract;

namespace OpcSubscriptions
{
    /// <summary>
    /// Provides extension methods for registering OPC UA subscription services
    /// in a dependency injection container.
    /// </summary>
    /// <remarks>
    /// This class integrates <see cref="IOpcSubscriptions"/> and its dependencies 
    /// into the Microsoft.Extensions.DependencyInjection pipeline. 
    /// The callback manager is also registered to ensure subscriptions 
    /// can properly manage monitored items.
    /// </remarks>
    public static class Dependency
    {
        /// <summary>
        /// Registers <see cref="IOpcSubscriptions"/> and its implementation
        /// (<see cref="OpcSubscriptionsManager"/>) as a singleton service.
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        /// <returns>The updated service collection.</returns>
        /// <remarks>
        /// This method also registers the callback manager by calling <c>AddCallbackManager()</c>.
        /// After registration, application services can request <see cref="IOpcSubscriptions"/>
        /// from the dependency injection container.
        /// </remarks>
        public static IServiceCollection AddSubscriptionManager(this IServiceCollection services)
        {
            return services
                .AddCallbackManager()
                .AddSingleton<IOpcSubscriptions, OpcSubscriptionsManager>();
        }
    }
}
