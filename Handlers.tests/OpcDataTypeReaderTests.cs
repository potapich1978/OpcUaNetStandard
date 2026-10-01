using Opc.Ua;
using Opc.Ua.Client;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the OpcDataTypeReader class.
    /// </summary>
    public class OpcDataTypeReaderTests
    {
        private const string DiametrNode = "ns=2;s=Dev.Diametr";
        private const string FlagsNode = "ns=2;s=Dev.Flags";
        private static readonly NodeId[] Nodes = { new(DiametrNode), new(FlagsNode) };

        private readonly ISession _session = Substitute.For<ISession>();
        private ReadValueIdCollection _read;

        /// <summary>
        /// Verifies that DataType and ValueRank attributes of every node are read in one request
        /// and resolved to the built-in type and value rank of the node.
        /// </summary>
        [Fact]
        public async Task ReadAsync_ReadsDataTypeAndValueRankOfEachNode()
        {
            // Arrange
            SetupRead(
                new DataValue(new Variant(DataTypeIds.Float)), new DataValue(new Variant(ValueRanks.Scalar)),
                new DataValue(new Variant(DataTypeIds.Boolean)), new DataValue(new Variant(ValueRanks.OneDimension)));
            using var cancellation = new CancellationTokenSource();

            // Act
            var dataTypes = await OpcDataTypeReader.ReadAsync(_session, Nodes, cancellation.Token);

            // Assert
            Assert.NotNull(_read);
            Assert.Equal(
                new[] { (DiametrNode, Attributes.DataType), (DiametrNode, Attributes.ValueRank),
                        (FlagsNode, Attributes.DataType), (FlagsNode, Attributes.ValueRank) },
                _read.Select(r => (r.NodeId.ToString(), r.AttributeId)));
            Assert.Equal(
                new[] { (BuiltInType.Float, ValueRanks.Scalar), (BuiltInType.Boolean, ValueRanks.OneDimension) },
                dataTypes.Select(t => (t.BuiltInType, t.ValueRank)));
            await _session.Received(1).ReadAsync(
                Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                Arg.Any<ReadValueIdCollection>(), cancellation.Token);
        }

        /// <summary>
        /// Verifies that a node whose attributes could not be read is reported without a concrete data type.
        /// </summary>
        [Fact]
        public async Task ReadAsync_WithUnreadableAttributes_ReportsNoConcreteType()
        {
            // Arrange
            SetupRead(new DataValue(StatusCodes.BadNodeIdUnknown), new DataValue(StatusCodes.BadNodeIdUnknown));

            // Act
            var dataTypes = await OpcDataTypeReader.ReadAsync(
                _session, new[] { new NodeId(DiametrNode) }, CancellationToken.None);

            // Assert
            var dataType = Assert.Single(dataTypes);
            Assert.Equal(BuiltInType.Null, dataType.BuiltInType);
            Assert.Equal(ValueRanks.Scalar, dataType.ValueRank);
        }

        private void SetupRead(params DataValue[] values)
        {
            _session.ReadAsync(Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                    Arg.Do<ReadValueIdCollection>(r => _read = r), Arg.Any<CancellationToken>())
                .Returns(new ReadResponse { Results = new DataValueCollection(values) });
        }
    }
}
