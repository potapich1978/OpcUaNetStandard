
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
    }
}
