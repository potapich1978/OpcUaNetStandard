using EventChannelBuilder;
using Microsoft.Extensions.DependencyInjection;
using OpcSessions;
using OpcSubscriptions;
using System.Reflection;

namespace Handlers
{
    /// <summary>
    /// Dependency injection extensions for OPC backend services.
    /// </summary>
    public static class Dependency
    {
        /// <summary>
        /// Registers OPC UA session manager, subscription manager, and event channel builder.
        /// </summary>
        /// <remarks>
        /// Automatically scans the assembly for event handlers of <see cref="Events.OpcCommandEvent"/>.
        /// </remarks>
        /// <param name="services">Service collection to register dependencies into.</param>
        /// <returns>Updated service collection with registered OPC backend services.</returns>
        public static IServiceCollection AddOpcBackend(this IServiceCollection services)
        {
            return services
                .AddOpcSessionManager()
                .AddSubscriptionManager()
                .AddEventChannelBuilder<Events.OpcCommandEvent>(Assembly.GetExecutingAssembly());
        }
    }
}
