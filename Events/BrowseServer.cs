using ChannelReader.Abstract;
using System;

namespace Events
{
    public sealed class BrowseServer : IGenericEvent<OpcCommandEvent>
    {
        /// <inheritdoc />
        public OpcCommandEvent EventType => OpcCommandEvent.Browse;

        /// <summary>
        /// Identifier of the OPC UA application (used to resolve the session).
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Full qulified opc node name for get child nodes.
        /// </summary>
        /// <remarks>
        /// for root level use null 
        /// </remarks>
        public string NodeFullName{ get; private set; }

        /// <summary>
        /// callback for action with each child node
        /// </summary>
        public Action<IOpcNodeInfo> NodeAction { get; set; }

        /// <summary>
        /// Creates a new <see cref="BrowseServer"/> event instance.
        /// </summary>
        /// <param name="appId">Application identifier.</param>
        /// <param name="nodeFullName">Full qulified opc node name for get child nodes. or null for root level</param>
        /// <param name="nodeAction">callback for action with each child node</param>
        public BrowseServer(string appId, string nodeFullName, Action<IOpcNodeInfo> nodeAction )
        {
            AppId = appId;
            NodeFullName = nodeFullName;
            NodeAction = nodeAction ?? throw new ArgumentNullException("parameter nodeAction can't be null");
        }
    }
}
