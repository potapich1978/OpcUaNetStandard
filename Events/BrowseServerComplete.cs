using ChannelReader.Abstract;
using System;
using System.Collections.Generic;

namespace Events
{
    /// <summary>
    /// Event for browsing one level of OPC UA nodes with the whole level delivered at once.
    /// </summary>
    public sealed class BrowseServerComplete : IGenericEvent<OpcCommandEvent>
    {
        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.BrowseComplete;

        /// <summary>
        /// Identifier of the OPC UA application (used to resolve the session).
        /// </summary>
        public string AppId { get; }

        /// <summary>
        /// Full qualified opc node name for get child nodes.
        /// </summary>
        /// <remarks>
        /// for root level use null
        /// </remarks>
        public string NodeFullName { get; }

        /// <summary>
        /// callback invoked once: child nodes and null on success, null and the error on failure
        /// </summary>
        public Action<IReadOnlyList<IOpcNodeInfo>, Exception> Completed { get; }

        /// <summary>
        /// Creates a new <see cref="BrowseServerComplete"/> event instance.
        /// </summary>
        /// <param name="appId">Application identifier.</param>
        /// <param name="nodeFullName">Full qualified opc node name for get child nodes. or null for root level</param>
        /// <param name="completed">callback invoked once with child nodes or with the error</param>
        public BrowseServerComplete(string appId, string nodeFullName, Action<IReadOnlyList<IOpcNodeInfo>, Exception> completed)
        {
            AppId = appId;
            NodeFullName = nodeFullName;
            Completed = completed ?? throw new ArgumentNullException(nameof(completed));
        }
    }
}
