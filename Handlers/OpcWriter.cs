using Opc.Ua;
using Opc.Ua.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Handlers
{
    /// <summary>
    /// Writes a value to the Value attribute of an OPC UA variable node.
    /// </summary>
    internal static class OpcWriter
    {
        /// <summary>
        /// Writes the value converted to the data type of the tag;
        /// throws when the value cannot be converted or the server rejects the write.
        /// </summary>
        public static async Task WriteAsync(ISession session, string tagId, object value, CancellationToken token)
        {
            var nodeId = new NodeId(tagId);
            var tagTypes = await OpcDataTypeReader.ReadAsync(session, new[] { nodeId }, token);

            var nodesToWrite = new WriteValueCollection
            {
                new WriteValue
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.Value,
                    // Only the value is sent: a server may reply BadWriteNotSupported to a write carrying timestamps.
                    Value = new DataValue(new Variant(OpcValueConverter.ToTagType(value, tagTypes[0])))
                }
            };

            var response = await session.WriteAsync(new RequestHeader(), nodesToWrite, token);
            ClientBase.ValidateResponse(response.Results, nodesToWrite);

            var status = response.Results[0];
            if (StatusCode.IsBad(status))
                throw new ServiceResultException(status);
        }
    }
}
