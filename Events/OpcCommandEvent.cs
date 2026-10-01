
namespace Events
{
    /// <summary>
    /// Defines all supported OPC UA command events.
    /// </summary>
    public enum OpcCommandEvent
    {
        /// <summary>
        /// Add a monitored item to a subscription.
        /// </summary>
        AddItemToSubscription,

        /// <summary>
        /// Register a new OPC UA session.
        /// </summary>
        RegisterSession,

        /// <summary>
        /// Remove a monitored item from a subscription.
        /// </summary>
        RemoveItemFromSubscription,

        /// <summary>
        /// Browse server nodes
        /// </summary>
        Browse,

        /// <summary>
        /// Browse server nodes and deliver the whole level at once
        /// </summary>
        BrowseComplete,

        /// <summary>
        /// Write a value to a tag
        /// </summary>
        WriteTagValue,

        /// <summary>
        /// Write a value to a tag and deliver the outcome to the caller
        /// </summary>
        WriteTagValueComplete
    }
}
