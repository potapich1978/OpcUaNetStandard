using ChannelReader.Abstract;

namespace Events
{

    /// <summary>
    /// Event for removing a monitored item from an OPC UA subscription.
    /// </summary>
    /// <remarks>
    /// Identifies the target subscription by application ID, tag ID, and event key.
    /// </remarks>
    public sealed class RemoveItemFromSubscription : IGenericEvent<OpcCommandEvent>
    {

        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.RemoveItemFromSubscription;

        /// <summary>
        /// Application identifier associated with the subscription.
        /// </summary>
        public string AppId { get; private set; }

        /// <summary>
        /// Identifier of the monitored tag to remove.
        /// </summary>
        public string TagId { get; private set; }

        /// <summary>
        /// Event key for identifying the monitored item.
        /// </summary>
        public string EventKey { get; private set; }

        /// <summary>
        /// Creates a new <see cref="RemoveItemFromSubscription"/> event instance.
        /// </summary>
        /// <param name="appId">Application identifier.</param>
        /// <param name="tagId">Tag (node) identifier.</param>
        /// <param name="eventKey">Event key for the monitored item.</param>
        public RemoveItemFromSubscription(string appId, string tagId, string eventKey)
        {
            AppId = appId;
            TagId = tagId;
            EventKey = eventKey;
        }
    }
}
