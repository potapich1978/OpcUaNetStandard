using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    /// <summary>
    /// Browses one level of the OPC UA address space and resolves value data types of variable nodes.
    /// </summary>
    internal static class OpcBrowser
    {
        /// <summary>
        /// Child nodes (folders and variables) of the node, or of ObjectsFolder when the name is empty.
        /// </summary>
        public static async Task<List<OpcNodeInfo>> BrowseAsync(ISession session, string nodeFullName, CancellationToken token)
        {
            var parentNode = string.IsNullOrEmpty(nodeFullName)
                ? ObjectIds.ObjectsFolder
                : new NodeId(nodeFullName);

            var children = await BrowseChildren(session, parentNode, token);
            var dataTypes = await ReadDataTypes(session, children, token);

            var nodes = new List<OpcNodeInfo>(children.Count);
            for (var i = 0; i < children.Count; i++)
                nodes.Add(new OpcNodeInfo(children[i], dataTypes[i]));

            return nodes;
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
