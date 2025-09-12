using Opc.Ua;
using Opc.Ua.Client;
using OpcCallbacks.Abstract;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace OpcCallbacks
{
    /// <summary>
    /// Provides a thread-safe implementation of <see cref="IOpcCallbackManager"/> for managing
    /// monitored items and callbacks within OPC UA subscriptions.
    /// </summary>
    /// <remarks>
    /// This class maintains two internal dictionaries:
    /// <list type="bullet">
    /// <item><description><c>_callbacks</c> stores event callbacks keyed by event keys.</description></item>
    /// <item><description><c>_itemsCallbacks</c> tracks the number of callbacks registered per monitored item.</description></item>
    /// </list>
    /// When the reference count for a monitored item reaches zero, the item is removed from the subscription.
    /// </remarks>
    internal sealed class OpcCallbacksManager : IOpcCallbackManager
    {
        /// <summary>
        /// Stores registered callbacks keyed by event key.
        /// </summary>
        /// <remarks>
        /// Used to manage callback removal and avoid memory leaks 
        /// when monitored items are detached from subscriptions.
        /// </remarks>
        private readonly ConcurrentDictionary<string, MonitoredItemNotificationEventHandler> _callbacks;

        /// <summary>
        /// Tracks the number of callbacks associated with each monitored item (tag).
        /// </summary>
        /// <remarks>
        /// Represent thread-safe methods for register/unregister callbacks for OPC Item (tagId).
        /// When the count of callbacks drops to zero, the monitored item is removed from the subscription.
        /// </remarks>
        private readonly IOpcTagCallbacksCount _itemsCallbacks;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcCallbacksManager"/> class.
        /// </summary>
        /// <remarks>
        /// Creates internal dictionaries for callback management. 
        /// </remarks>
        public OpcCallbacksManager(IOpcTagCallbacksCount itemsCallbacks)
        {
            _callbacks = new ConcurrentDictionary<string, MonitoredItemNotificationEventHandler>();
            _itemsCallbacks = itemsCallbacks;
        }

        /// <inheritdoc />
        /// <remarks>
        /// If the monitored item already exists in the subscription, the callback is added
        /// and the reference counter is incremented. If not, a new monitored item is created
        /// with default settings (sampling interval, queue size, discard oldest).
        /// </remarks>
        public Task AddItemToSubscription(Subscription subscription, string tagId, string eventKey, MonitoredItemNotificationEventHandler callback)
        {
            var item = subscription.MonitoredItems.FirstOrDefault(e => e.DisplayName == tagId);
            if (item != null)
            {
                item.Notification += callback;
                _itemsCallbacks.RegisterCallback(tagId);
                return Task.CompletedTask;
            }
            return Task.Run(() =>
            {
                var nodeId = new NodeId(tagId);
                item = new MonitoredItem(subscription.DefaultItem)
                {
                    DisplayName = tagId,
                    StartNodeId = nodeId,
                    SamplingInterval = 1000,
                    QueueSize = 10,
                    DiscardOldest = true
                };
                subscription.AddItem(item);
                item.Notification += callback;
                _itemsCallbacks.RegisterCallback(tagId);
            });
        }

        /// <inheritdoc />
        /// <remarks>
        /// Removes the callback associated with the given event key. 
        /// If the monitored item has no remaining callbacks, the item itself is removed 
        /// from the subscription to release resources.
        /// </remarks>
        public Task RemoveItemFromSubscription(Subscription subscription, string tagId, string eventKey)
        {
            return Task.Run(() =>
            {
                _callbacks.TryRemove(eventKey, out var callback);

                var item = subscription.MonitoredItems
                    .FirstOrDefault(i => i.DisplayName == tagId);

                var callbackCount = _itemsCallbacks.UnregisterCallback(tagId);

                if (item != null)
                {
                    item.Notification -= callback;
                    if(callbackCount == 0)
                        subscription.RemoveItem(item);
                }
            });
        }
    }
}
