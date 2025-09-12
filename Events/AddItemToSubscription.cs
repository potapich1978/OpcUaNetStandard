using ChannelReader.Abstract;
using Opc.Ua.Client;


namespace Events
{
    /// <summary>
    /// Event for adding a monitored item to an OPC UA subscription.
    /// </summary>
    /// <remarks>
    /// Encapsulates the required parameters: application ID, tag ID, event key, and
    /// a callback handler for receiving monitored item notifications.
    /// </remarks>
    public sealed class AddItemToSubscription : IGenericEvent<OpcCommandEvent>
    {
        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.AddItemToSubscription;

        /// <summary>
        /// Identifier of the OPC UA application (used to resolve the session).
        /// </summary>
        public string AppId { get; private set; }

        /// <summary>
        /// Identifier of the monitored tag (node).
        /// </summary>
        public string TagId { get; private set; }

        /// <summary>
        /// Event key for distinguishing callbacks.
        /// </summary>
        public string EventKey { get; private set; }

        /// <summary>
        /// Callback to invoke when monitored item notifications are received.
        /// <see cref="MonitoredItemNotificationEventHandler"/>
        /// </summary>
        public MonitoredItemNotificationEventHandler Callback { get; private set; }

        /// <summary>
        /// Creates a new <see cref="AddItemToSubscription"/> event instance.
        /// </summary>
        /// <param name="appId">Application identifier.</param>
        /// <param name="tagId">Tag (node) identifier.</param>
        /// <param name="eventKey">Unique event key for callback management.</param>
        /// <param name="callback">Handler for monitored item notifications.</param>
        public AddItemToSubscription(string appId, string tagId, string eventKey, MonitoredItemNotificationEventHandler callback)
        {
            AppId = appId;
            TagId = tagId;
            Callback = callback;
            EventKey = eventKey;
        }
    }
}
