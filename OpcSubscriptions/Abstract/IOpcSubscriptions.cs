using Opc.Ua.Client;
using System.Threading.Tasks;

namespace OpcSubscriptions.Abstract
{
    /// <summary>
    /// Provides methods for managing OPC UA subscriptions and their monitored items.
    /// </summary>
    /// <remarks>
    /// This interface abstracts subscription lifecycle management, 
    /// including creation, monitored item registration, and cleanup.
    /// </remarks>
    public interface IOpcSubscriptions
    {

        /// <summary>
        /// Adds a subscription for the specified application and attaches a monitored item callback.
        /// </summary>
        /// <param name="session">The OPC UA session associated with the subscription.</param>
        /// <param name="appId">The application identifier used as a subscription key.</param>
        /// <param name="tagId">The identifier of the tag or NodeId to monitor.</param>
        /// <param name="eventKey">A unique key identifying the callback event.</param>
        /// <param name="callback">The callback to invoke when notifications are received.</param>
        /// <remarks>
        /// If a subscription does not exist for the given <paramref name="appId"/>, 
        /// a new subscription will be created and registered with the session. 
        /// The callback is added to the subscription through the callback manager.
        /// </remarks>
        Task AddSubscriprion(ISession session, string appId, string tagId, string eventKey, 
                             MonitoredItemNotificationEventHandler callback);

        /// <summary>
        /// Removes a monitored item from the specified subscription.
        /// </summary>
        /// <param name="appId">The application identifier of the subscription.</param>
        /// <param name="tagId">The identifier of the monitored tag.</param>
        /// <param name="eventKey">The unique key identifying the callback event.</param>
        /// <remarks>
        /// If the subscription becomes empty after removal, it will be disposed and removed.
        /// </remarks>
        Task RemoveItemFromSubscription(string appId, string tagId, string eventKey);

        /// <summary>
        /// Removes and disposes the subscription associated with the given application ID.
        /// </summary>
        /// <param name="appId">The application identifier of the subscription to remove.</param>
        /// <remarks>
        /// All monitored items and callbacks associated with the subscription are released. 
        /// This method is typically used during application shutdown or cleanup.
        /// </remarks>
        void RemoveSubscriprion(string appId);
    }
}
