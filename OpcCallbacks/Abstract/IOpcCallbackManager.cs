using Opc.Ua.Client;
using System.Threading.Tasks;

namespace OpcCallbacks.Abstract
{
    /// <summary>
    /// Provides methods to manage OPC UA monitored item callbacks within a subscription.
    /// </summary>
    /// <remarks>
    /// This interface abstracts adding and removing monitored items and their associated callbacks.
    /// Implementations are expected to manage multiple callbacks for the same tag and handle 
    /// subscription updates safely.
    /// </remarks>
    public interface IOpcCallbackManager
    {
        /// <summary>
        /// Adds a monitored item to a subscription and attaches a notification callback.
        /// </summary>
        /// <param name="subscription">The subscription to which the item belongs.</param>
        /// <param name="tagId">The tag identifier or NodeId to monitor.</param>
        /// <param name="eventKey">A unique key identifying the callback event.</param>
        /// <param name="callback">The callback to invoke when notifications are received.</param>
        /// <remarks>
        /// If the monitored item already exists, the callback will be attached to it and 
        /// the internal reference counter will be incremented.
        /// Otherwise, a new monitored item will be created, added to the subscription, 
        /// and the callback will be associated with it.
        /// </remarks>
        Task AddItemToSubscription(Subscription subscription, string tagId, string eventKey, MonitoredItemNotificationEventHandler callback);

        /// <summary>
        /// Removes a monitored item callback from a subscription.
        /// </summary>
        /// <param name="subscription">The subscription from which the item is removed.</param>
        /// <param name="tagId">The tag identifier or NodeId of the monitored item.</param>
        /// <param name="eventKey">The unique key identifying the callback event.</param>
        /// <remarks>
        /// If no more callbacks remain for the monitored item, the item itself will be removed 
        /// from the subscription. Otherwise, only the specified callback is detached.
        /// </remarks>
        Task RemoveItemFromSubscription(Subscription subscription, string tagId, string eventKey);
    }
}
