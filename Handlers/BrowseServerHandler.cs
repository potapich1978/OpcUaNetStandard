using ChannelReader.Abstract;
using EventLogger;
using Events;
using Opc.Ua;
using Opc.Ua.Client;
using OpcSessions.Abstract;
using System;
using System.Collections.Generic;
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
        /// Processes the <see cref="BrowseServer"/> event asynchronously.
        /// </summary>
        /// <remarks>
        /// Validates the event type, checks for session registration, browses child nodes
        /// and reads value data type of each variable node.
        /// Logs appropriate errors if the session does not exist or event type is unsupported.
        /// </remarks>
        /// <param name="event">The generic OPC UA command event.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async Task HandleAsync(IGenericEvent<OpcCommandEvent> @event, CancellationToken token = default)
        {
            if (!(@event is BrowseServer browseCommand))
            {
                _logger.LogError($"BrowseServerHandler: incoming event unsupportable" +
                                 $" {@event.GetType().FullName}");
                return;
            }

            var session = _sessions.GetSession(browseCommand.AppId);
            if (session == null)
            {
                _logger.LogError($"BrowseServerHandler: Session for app " +
                                 $"{browseCommand.AppId} not registered. create session before browse server structure");
                return;
            }

            var parentNode = string.IsNullOrEmpty(browseCommand.NodeFullName)
                ? ObjectIds.ObjectsFolder
                : new NodeId(browseCommand.NodeFullName);

            var children = await BrowseChildren(session, parentNode, token);
            var dataTypes = await ReadDataTypes(session, children, token);

            for (var i = 0; i < children.Count; i++)
                browseCommand.NodeAction(new OpcNodeInfo(children[i], dataTypes[i]));
        }

        private static async Task<List<ReferenceDescription>> BrowseChildren(ISession session, NodeId parentNode, CancellationToken token)
        {
            var nodesToBrowse = new BrowseDescriptionCollection
            {
                new BrowseDescription
                {
                    NodeId = parentNode,
                    BrowseDirection = BrowseDirection.Forward,
                    ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                    IncludeSubtypes = true,
                    NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable),
                    ResultMask = (uint)BrowseResultMask.All,
                }
            };

            var response = await session.BrowseAsync(new RequestHeader(), null, 0, nodesToBrowse, token);

            var children = new List<ReferenceDescription>();
            var result = FirstResult(response.Results);
            while (result != null)
            {
                children.AddRange(result.References);
                if (result.ContinuationPoint == null || result.ContinuationPoint.Length == 0)
                    break;

                var next = await session.BrowseNextAsync(
                    new RequestHeader(),
                    false,
                    new ByteStringCollection { result.ContinuationPoint },
                    token);

                result = FirstResult(next.Results);
            }

            return children;
        }

        private static BrowseResult FirstResult(BrowseResultCollection results)
            => results != null && results.Count > 0 ? results[0] : null;

        private static async Task<Type[]> ReadDataTypes(ISession session, List<ReferenceDescription> nodes, CancellationToken token)
        {
            var dataTypes = new Type[nodes.Count];
            var attributesToRead = new ReadValueIdCollection();
            var variableIndexes = new List<int>();

            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].NodeClass != NodeClass.Variable)
                    continue;

                var nodeId = (NodeId)nodes[i].NodeId;
                attributesToRead.Add(new ReadValueId { NodeId = nodeId, AttributeId = Attributes.DataType });
                attributesToRead.Add(new ReadValueId { NodeId = nodeId, AttributeId = Attributes.ValueRank });
                variableIndexes.Add(i);
            }

            if (variableIndexes.Count == 0)
                return dataTypes;

            var response = await session.ReadAsync(new RequestHeader(), 0, TimestampsToReturn.Neither, attributesToRead, token);

            for (var i = 0; i < variableIndexes.Count; i++)
            {
                var dataTypeId = response.Results[2 * i].Value as NodeId;
                var valueRank = response.Results[2 * i + 1].Value as int? ?? ValueRanks.Scalar;
                var builtInType = TypeInfo.GetBuiltInType(dataTypeId, session.TypeTree);
                dataTypes[variableIndexes[i]] = TypeInfo.GetSystemType(builtInType, valueRank);
            }

            return dataTypes;
        }
    }
}
