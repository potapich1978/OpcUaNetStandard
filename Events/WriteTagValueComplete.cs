using ChannelReader.Abstract;
using System;

namespace Events
{
    /// <summary>
    /// Event for writing a value to an OPC UA tag with the outcome delivered to a callback.
    /// </summary>
    public sealed class WriteTagValueComplete : IGenericEvent<OpcCommandEvent>
    {
        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.WriteTagValueComplete;

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
        /// callback invoked once: null on success, the error on failure
        /// </summary>
        public Action<Exception> Completed { get; }

        /// <summary>
        /// Creates a new <see cref="WriteTagValueComplete"/> event instance.
        /// </summary>
        /// <param name="appId">Application identifier.</param>
        /// <param name="tagId">Tag (node) identifier.</param>
        /// <param name="value">Value to write; it is converted to the data type of the tag.</param>
        /// <param name="completed">callback invoked once with null or with the error</param>
        public WriteTagValueComplete(string appId, string tagId, object value, Action<Exception> completed)
        {
            AppId = appId;
            TagId = tagId;
            Value = value;
            Completed = completed ?? throw new ArgumentNullException(nameof(completed));
        }
    }
}
