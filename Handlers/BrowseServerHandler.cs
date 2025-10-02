using ChannelReader.Abstract;
using EventLogger;
using Events;
using Opc.Ua;
using OpcSessions.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    internal sealed class BrowseServerHandler : IGenericEventHandler<OpcCommandEvent>
    {
        public OpcCommandEvent EventType => OpcCommandEvent.Browse;

        /// <summary>
        /// Logger for reporting errors and diagnostic information.
        /// </summary>
        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// OPC UA sessions manager.
        /// </summary>
        private readonly IOpcSessionsManager _sessions;


        /// <summary>
        /// Initializes a new instance of <see cref="BrowseServerHandler"/>.
        /// </summary>
        /// <param name="logger">Event dispatcher logger.</param>
        /// <param name="sessions">OPC UA sessions manager.</param>
        public BrowseServerHandler(IGenericEventDispatcherLogger logger, IOpcSessionsManager sessions)
        {
            _logger = logger;
            _sessions = sessions;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the <see cref="BrowseServerHandler"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Validates the event type, checks for session registration, and brows node.
        /// Logs appropriate errors if the session does not exist or event type is unsupported.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {

            if (!(@event is BrowseServer browseCommand))
                _logger.LogError($"BrowseServerHandler: incoming event unsupportable" +
                                 $" {@event.GetType().FullName}");

            else
            {
                var session = _sessions.GetSession(browseCommand.AppId);
                if (session == null)
                {
                    _logger.LogError($"BrowseServerHandler: Session for app " +
                                     $"{browseCommand.AppId} not registered. create session before browse server structure");
                }
                else
                {
                    var parentNode = string.IsNullOrEmpty(browseCommand.NodeFullName)
                        ? ObjectIds.ObjectsFolder
                        : new NodeId(browseCommand.NodeFullName);
                    
                    var nodesToBrowse = new BrowseDescriptionCollection
                    {
                        new BrowseDescription
                        {
                            NodeId = (NodeId)new ExpandedNodeId(parentNode),
                            BrowseDirection = BrowseDirection.Forward,
                            ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                            IncludeSubtypes = true,
                            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable),
                            ResultMask = (uint)BrowseResultMask.All,
                        }
                    };

                    var response = await session.BrowseAsync(
                        new RequestHeader(),
                        null,   
                        0,      
                        nodesToBrowse,
                        token);
                    

                    if (response.Results != null && response.Results.Count > 0)
                    {
                        var br = response.Results[0];
                        foreach (var rd in br.References)
                            browseCommand.NodeAction(new OpcNodeInfo(rd));

                        while (br.ContinuationPoint != null && br.ContinuationPoint.Length > 0)
                        {
                            var next = await session.BrowseNextAsync(
                                new RequestHeader(),
                                false,
                                new ByteStringCollection { br.ContinuationPoint },
                                token);

                            if (next.Results != null && next.Results.Count > 0)
                            {
                                var nextBr = next.Results[0];
                                foreach (var rd in nextBr.References)
                                    browseCommand.NodeAction(new OpcNodeInfo(rd));

                                br = nextBr;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
