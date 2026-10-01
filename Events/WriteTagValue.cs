using ChannelReader.Abstract;

namespace Events
{
    /// <summary>
    /// Event for writing a value to an OPC UA tag without delivering the outcome to the caller.
    /// </summary>
    /// <remarks>
    /// A missing session, a value that cannot be converted or a write failure is only logged.
    /// Use <see cref="WriteTagValueComplete"/> when the caller needs the outcome.
    /// </remarks>
    public sealed class WriteTagValue : IGenericEvent<OpcCommandEvent>
    {
        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.WriteTagValue;

        /// <summary>
        /// Identifier of the OPC UA application (used to resolve the session).
        /// </summary>
        public string AppId { get; }

        /// <summary>
        /// Identifier of the tag (node) to write.
        /// </summary>
        public string TagId { get; }

        /// <summary>
        /// Value to write.
        /// </summary>
        /// <remarks>
        /// converted to the data type of the tag before the write
        /// </remarks>
        public object Value { get; }

        /// <summary>
        /// Creates a new <see cref="WriteTagValue"/> event instance.
        /// </summary>
        /// <param name="appId">Application identifier.</param>
        /// <param name="tagId">Tag (node) identifier.</param>
        /// <param name="value">Value to write; it is converted to the data type of the tag.</param>
        public WriteTagValue(string appId, string tagId, object value)
        {
            AppId = appId;
            TagId = tagId;
            Value = value;
        }
    }
}
