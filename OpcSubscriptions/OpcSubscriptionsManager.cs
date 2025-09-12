using Opc.Ua.Client;
using OpcCallbacks.Abstract;
using OpcSubscriptions.Abstract;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace OpcSubscriptions
{
    /// <summary>
    /// Provides a thread-safe implementation of <see cref="IOpcSubscriptions"/> 
    /// for managing OPC UA subscriptions and their monitored items.
    /// </summary>
    /// <remarks>
    /// This class stores subscriptions in a <see cref="ConcurrentDictionary{TKey,TValue}"/>,
    /// keyed by application ID. It delegates monitored item management to 
    /// an <see cref="IOpcCallbackManager"/> implementation.
    /// </remarks>
    internal class OpcSubscriptionsManager: IOpcSubscriptions
    {
        /// <summary>
        /// Internal storage for subscriptions, keyed by application ID.
        /// </summary>
        /// <remarks>
        /// Subscriptions are automatically created on demand and disposed 
        /// when they contain no more monitored items.
        /// </remarks>
        private readonly ConcurrentDictionary<string, Subscription> _subscriptions;

        /// <summary>
        /// Manages monitored item callbacks for the subscriptions.
        /// </summary>
        /// <remarks>
        /// The callback manager handles attaching and detaching event handlers
        /// for monitored items across different subscriptions.
        /// </remarks>
        private readonly IOpcCallbackManager _callbackManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcSubscriptionsManager"/> class.
        /// </summary>
        /// <param name="callbackManager">The callback manager used to handle monitored items.</param>
        /// <remarks>
        /// The provided <paramref name="callbackManager"/> is used to ensure consistent 
        /// handling of notification callbacks across all managed subscriptions.
        /// </remarks>
        public OpcSubscriptionsManager(IOpcCallbackManager callbackManager)
        {
            _callbackManager = callbackManager;
            _subscriptions = new ConcurrentDictionary<string, Subscription>();
        }

        /// <inheritdoc />
        /// <remarks>
        /// If a subscription for the given <paramref name="appId"/> does not exist, 
        /// a new one is created with default publishing interval (1000 ms). 
        /// The subscription is added to the session and monitored item callbacks are registered.
        /// At the moment this method only attaches the very first monitored item 
        /// when a new subscription is created. Subsequent calls with the same 
        /// <paramref name="appId"/> will **not** add additional monitored items 
        /// to the existing subscription. To support multiple monitored items per subscription, 
        /// the logic must be extended.
        /// </remarks>
        public async Task AddSubscriprion(ISession session, string appId, string tagId, string eventKey,
                                                        MonitoredItemNotificationEventHandler callback)
        {
            if (!(_subscriptions.TryGetValue(appId, out var subscription)))
            {
                subscription = new Subscription(session.DefaultSubscription)
                {
                    PublishingInterval = 1000,
                    PublishingEnabled = true
                };
                session.AddSubscription(subscription);
                await CreateSubscriptionAsync(subscription);
                await _callbackManager.AddItemToSubscription(subscription, tagId, eventKey, callback);
                await ApplySubscriptionChanges(subscription);
                _subscriptions.AddOrUpdate(appId, subscription, (k, s) => subscription);
            }
            await Task.CompletedTask;
        }

        internal virtual async Task CreateSubscriptionAsync(Subscription subscription)
            => await subscription.CreateAsync();

        internal virtual async Task ApplySubscriptionChanges(Subscription subscription)
            => await subscription.ApplyChangesAsync();

        /// <inheritdoc />
        /// <remarks>
        /// The monitored item is removed via the callback manager. 
        /// If the subscription has no monitored items after removal, 
        /// it is disposed and removed from the dictionary.
        /// </remarks>
        public Task RemoveItemFromSubscription(string appId, string tagId, string eventKey)
        {
            return Task.Run(() =>
            {
                if (_subscriptions.TryGetValue(appId, out var subscription))
                {
                    _callbackManager.RemoveItemFromSubscription(subscription, tagId, eventKey);
                    if(subscription.MonitoredItemCount == 0)
                    {
                        _subscriptions.TryRemove(appId, out _);
                        subscription.Dispose();
                    }
                }
            });
        }

        /// <inheritdoc />
        /// <remarks>
        /// Removes and disposes the entire subscription associated with the given <paramref name="appId"/>.
        /// This is useful when shutting down or explicitly cleaning up resources for a specific client.
        /// </remarks>
        public void RemoveSubscriprion(string appId)
        {
            if (_subscriptions.TryRemove(appId, out var subscription))
            {
                subscription.Dispose();
            }
        }
    }
}
