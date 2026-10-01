using Opc.Ua;
using Opc.Ua.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    /// <summary>
    /// Reads value data types of OPC UA variable nodes.
    /// </summary>
    internal static class OpcDataTypeReader
    {
        /// <summary>
        /// Data types of the nodes, resolved from their DataType and ValueRank attributes.
        /// </summary>
        public static async Task<TypeInfo[]> ReadAsync(
            ISession session, IReadOnlyList<NodeId> nodeIds, CancellationToken token)
        {
            var attributesToRead = new ReadValueIdCollection();
            foreach (var nodeId in nodeIds)
            {
                attributesToRead.Add(new ReadValueId { NodeId = nodeId, AttributeId = Attributes.DataType });
                attributesToRead.Add(new ReadValueId { NodeId = nodeId, AttributeId = Attributes.ValueRank });
            }

            var response = await session.ReadAsync(
                new RequestHeader(), 0, TimestampsToReturn.Neither, attributesToRead, token);

            var dataTypes = new TypeInfo[nodeIds.Count];
            for (var i = 0; i < nodeIds.Count; i++)
            {
                var dataTypeId = response.Results[2 * i].Value as NodeId;
                var valueRank = response.Results[2 * i + 1].Value as int? ?? ValueRanks.Scalar;
                dataTypes[i] = new TypeInfo(TypeInfo.GetBuiltInType(dataTypeId, session.TypeTree), valueRank);
            }

            return dataTypes;
        }
    }
}
