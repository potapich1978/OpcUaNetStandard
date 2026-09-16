using Opc.Ua;
using Opc.Ua.Client;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the OpcBrowser class.
    /// </summary>
    public class OpcBrowserTests
    {
        private const string DeviceNode = "ns=2;s=Dev";
        private const string DiametrNode = "ns=2;s=Dev.Diametr";
        private const string FlagsNode = "ns=2;s=Dev.Flags";
        private static readonly byte[] ContinuationPoint = { 1, 2, 3 };
        private static readonly string[] TwoPagesNames = { "A", "B" };

        private readonly ISession _session = Substitute.For<ISession>();
        private BrowseDescriptionCollection _browsed;
        private ByteStringCollection _continued;
        private ReadValueIdCollection _read;

        /// <summary>
        /// Verifies that an empty node name browses ObjectsFolder and a folder node
        /// is reported without reading attributes.
        /// </summary>
        [Fact]
        public async Task BrowseAsync_WithEmptyNodeName_BrowsesObjectsFolderAndReportsFolder()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(null, Reference("ns=2;s=Folder", "Folder", NodeClass.Object)));

            // Act
            var nodes = await OpcBrowser.BrowseAsync(_session, null, CancellationToken.None);

            // Assert
            Assert.NotNull(_browsed);
            Assert.Equal(ObjectIds.ObjectsFolder, Assert.Single(_browsed).NodeId);
            var node = Assert.Single(nodes);
            Assert.Equal("Folder", node.DisplayName);
            Assert.Equal("ns=2;s=Folder", node.FullName);
            Assert.True(node.IsFolder);
            Assert.False(node.Writable);
            Assert.Null(node.DataType);
            await _session.DidNotReceive().ReadAsync(
                Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                Arg.Any<ReadValueIdCollection>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Verifies that a given node is browsed and variable nodes get their data type
        /// from DataType and ValueRank attributes.
        /// </summary>
        [Fact]
        public async Task BrowseAsync_WithVariables_ReadsDataTypes()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(null,
                Reference(DiametrNode, "Diametr", NodeClass.Variable),
                Reference("ns=2;s=Dev.Sub", "Sub", NodeClass.Object),
                Reference(FlagsNode, "Flags", NodeClass.Variable)));
            SetupRead(
                DataTypeValues(DataTypeIds.Float, ValueRanks.Scalar),
                DataTypeValues(DataTypeIds.Boolean, ValueRanks.OneDimension));

            // Act
            var nodes = await OpcBrowser.BrowseAsync(_session, DeviceNode, CancellationToken.None);

            // Assert
            Assert.NotNull(_browsed);
            Assert.Equal(new NodeId(DeviceNode), Assert.Single(_browsed).NodeId);
            Assert.NotNull(_read);
            Assert.Equal(
                new[] { (DiametrNode, Attributes.DataType), (DiametrNode, Attributes.ValueRank),
                        (FlagsNode, Attributes.DataType), (FlagsNode, Attributes.ValueRank) },
                _read.Select(r => (r.NodeId.ToString(), r.AttributeId)));
            Assert.Equal(
                new[] { ("Diametr", typeof(float)), ("Sub", (Type)null), ("Flags", typeof(bool[])) },
                nodes.Select(n => (n.DisplayName, n.DataType)));
            Assert.All(nodes.Where(n => !n.IsFolder), n => Assert.True(n.Writable));
        }

        /// <summary>
        /// Verifies that a variable whose attributes could not be read is reported with Variant type.
        /// </summary>
        [Fact]
        public async Task BrowseAsync_WithUnreadableAttributes_ReportsVariant()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(null, Reference("ns=2;s=Dev.Bad", "Bad", NodeClass.Variable)));
            SetupRead(new[] { new DataValue(StatusCodes.BadNodeIdUnknown), new DataValue(StatusCodes.BadNodeIdUnknown) });

            // Act
            var nodes = await OpcBrowser.BrowseAsync(_session, DeviceNode, CancellationToken.None);

            // Assert
            Assert.Equal(typeof(Variant), Assert.Single(nodes).DataType);
        }

        /// <summary>
        /// Verifies that browse follows continuation points until the server returns an empty one.
        /// </summary>
        [Fact]
        public async Task BrowseAsync_WithContinuationPoint_BrowsesNextUntilEmptyPoint()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(ContinuationPoint, Reference("ns=2;s=A", "A", NodeClass.Object)));
            SetupBrowseNext(new BrowseResultCollection
            {
                BrowseResultOf(Array.Empty<byte>(), Reference("ns=2;s=B", "B", NodeClass.Object))
            });

            // Act
            var nodes = await OpcBrowser.BrowseAsync(_session, null, CancellationToken.None);

            // Assert
            Assert.NotNull(_continued);
            Assert.Equal(ContinuationPoint, Assert.Single(_continued));
            Assert.Equal(TwoPagesNames, nodes.Select(n => n.DisplayName));
        }

        /// <summary>
        /// Verifies that browse stops when BrowseNext returns no results.
        /// </summary>
        [Fact]
        public async Task BrowseAsync_WhenBrowseNextReturnsNoResults_StopsBrowsing()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(ContinuationPoint, Reference("ns=2;s=A", "A", NodeClass.Object)));
            SetupBrowseNext(new BrowseResultCollection());

            // Act
            var nodes = await OpcBrowser.BrowseAsync(_session, null, CancellationToken.None);

            // Assert
            Assert.Equal("A", Assert.Single(nodes).DisplayName);
        }

        /// <summary>
        /// Verifies that no nodes are returned when browse returns no results.
        /// </summary>
        [Fact]
        public async Task BrowseAsync_WhenBrowseReturnsNoResults_ReturnsNothing()
        {
            // Arrange
            SetupBrowse(null);

            // Act
            var nodes = await OpcBrowser.BrowseAsync(_session, null, CancellationToken.None);

            // Assert
            Assert.Empty(nodes);
        }

        private void SetupBrowse(BrowseResult result)
        {
            var results = result == null ? null : new BrowseResultCollection { result };
            _session.BrowseAsync(Arg.Any<RequestHeader>(), Arg.Any<ViewDescription>(), Arg.Any<uint>(),
                    Arg.Do<BrowseDescriptionCollection>(b => _browsed = b), Arg.Any<CancellationToken>())
                .Returns(new BrowseResponse { Results = results });
        }

        private void SetupBrowseNext(BrowseResultCollection results)
        {
            _session.BrowseNextAsync(Arg.Any<RequestHeader>(), Arg.Any<bool>(),
                    Arg.Do<ByteStringCollection>(c => _continued = c), Arg.Any<CancellationToken>())
                .Returns(new BrowseNextResponse { Results = results });
        }

        private void SetupRead(params DataValue[][] values)
        {
            _session.ReadAsync(Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                    Arg.Do<ReadValueIdCollection>(r => _read = r), Arg.Any<CancellationToken>())
                .Returns(new ReadResponse { Results = new DataValueCollection(values.SelectMany(v => v)) });
        }

        private static BrowseResult BrowseResultOf(byte[] continuationPoint, params ReferenceDescription[] references)
            => new()
            {
                ContinuationPoint = continuationPoint,
                References = new ReferenceDescriptionCollection(references)
            };

        private static ReferenceDescription Reference(string nodeId, string displayName, NodeClass nodeClass)
            => new()
            {
                NodeId = new ExpandedNodeId(new NodeId(nodeId)),
                DisplayName = new LocalizedText(displayName),
                NodeClass = nodeClass
            };

        private static DataValue[] DataTypeValues(NodeId dataTypeId, int valueRank)
            => new[] { new DataValue(new Variant(dataTypeId)), new DataValue(new Variant(valueRank)) };
    }
}
