using ChannelReader.Abstract;
using EventLogger;
using Events;
using Opc.Ua;
using Opc.Ua.Client;
using OpcSessions.Abstract;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the BrowseServerHandler class.
    /// </summary>
    public class BrowseServerHandlerTests
    {
        private const string AppId = "testApp";
        private const string DeviceNode = "ns=2;s=Dev";
        private const string DiametrNode = "ns=2;s=Dev.Diametr";
        private const string FlagsNode = "ns=2;s=Dev.Flags";
        private static readonly byte[] ContinuationPoint = { 1, 2, 3 };
        private static readonly string[] TwoPagesNames = { "A", "B" };

        private readonly IGenericEventDispatcherLogger _logger;
        private readonly IOpcSessionsManager _sessions;
        private readonly ISession _session;
        private readonly BrowseServerHandler _handler;
        private readonly List<IOpcNodeInfo> _nodes = new();
        private BrowseDescriptionCollection _browsed;
        private ByteStringCollection _continued;
        private ReadValueIdCollection _read;

        /// <summary>
        /// Initializes test dependencies using mocks.
        /// </summary>
        public BrowseServerHandlerTests()
        {
            _logger = Substitute.For<IGenericEventDispatcherLogger>();
            _sessions = Substitute.For<IOpcSessionsManager>();
            _session = Substitute.For<ISession>();
            _sessions.GetSession(AppId).Returns(_session);
            _handler = new BrowseServerHandler(_logger, _sessions);
        }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnBrowse()
        {
            // Act & Assert
            Assert.Equal(OpcCommandEvent.Browse, _handler.EventType);
        }

        /// <summary>
        /// Verifies that HandleAsync logs an error when an unsupported event type is received.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithUnsupportedEvent_LogsError()
        {
            // Arrange
            var unsupportedEvent = Substitute.For<IGenericEvent<OpcCommandEvent>>();

            // Act
            await _handler.HandleAsync(unsupportedEvent);

            // Assert
            _logger.Received(1).LogError(Arg.Is<string>(s => s.Contains("unsupportable")));
            _sessions.DidNotReceive().GetSession(Arg.Any<string>());
        }

        /// <summary>
        /// Verifies that HandleAsync logs an error and does not browse when session is not registered.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithoutSession_LogsError()
        {
            // Arrange
            _sessions.GetSession("unknownApp").Returns((ISession)null);
            var browseCommand = new BrowseServer("unknownApp", null, _nodes.Add);

            // Act
            await _handler.HandleAsync(browseCommand);

            // Assert
            _logger.Received(1).LogError(Arg.Is<string>(s => s.Contains("not registered")));
            await _session.DidNotReceive().BrowseAsync(
                Arg.Any<RequestHeader>(), Arg.Any<ViewDescription>(), Arg.Any<uint>(),
                Arg.Any<BrowseDescriptionCollection>(), Arg.Any<CancellationToken>());
            Assert.Empty(_nodes);
        }

        /// <summary>
        /// Verifies that root level browse starts from ObjectsFolder and a folder node
        /// is reported without reading attributes.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithNullNodeName_BrowsesObjectsFolderAndReportsFolder()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(null, Reference("ns=2;s=Folder", "Folder", NodeClass.Object)));
            var browseCommand = new BrowseServer(AppId, null, _nodes.Add);

            // Act
            await _handler.HandleAsync(browseCommand);

            // Assert
            Assert.NotNull(_browsed);
            Assert.Equal(ObjectIds.ObjectsFolder, Assert.Single(_browsed).NodeId);
            var node = Assert.Single(_nodes);
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
        /// Verifies that sub level browse starts from the given node and variable nodes
        /// get their data type from DataType and ValueRank attributes.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithVariables_ReadsDataTypes()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(null,
                    Reference(DiametrNode, "Diametr", NodeClass.Variable),
                    Reference("ns=2;s=Dev.Sub", "Sub", NodeClass.Object),
                    Reference(FlagsNode, "Flags", NodeClass.Variable)));
            SetupRead(DataTypeValues(DataTypeIds.Float, ValueRanks.Scalar),
                DataTypeValues(DataTypeIds.Boolean, ValueRanks.OneDimension));
            var browseCommand = new BrowseServer(AppId, DeviceNode, _nodes.Add);

            // Act
            await _handler.HandleAsync(browseCommand);

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
                _nodes.Select(n => (n.DisplayName, n.DataType)));
            Assert.All(_nodes.Where(n => !n.IsFolder), n => Assert.True(n.Writable));
        }

        /// <summary>
        /// Verifies that a variable whose attributes could not be read is reported with Variant type.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithUnreadableAttributes_ReportsVariant()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(null, Reference("ns=2;s=Dev.Bad", "Bad", NodeClass.Variable)));
            SetupRead(new[] { new DataValue(StatusCodes.BadNodeIdUnknown), new DataValue(StatusCodes.BadNodeIdUnknown) });
            var browseCommand = new BrowseServer(AppId, DeviceNode, _nodes.Add);

            // Act
            await _handler.HandleAsync(browseCommand);

            // Assert
            Assert.Equal(typeof(Variant), Assert.Single(_nodes).DataType);
        }

        /// <summary>
        /// Verifies that browse follows continuation points until the server returns an empty one.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithContinuationPoint_BrowsesNextUntilEmptyPoint()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(ContinuationPoint, Reference("ns=2;s=A", "A", NodeClass.Object)));
            SetupBrowseNext(new BrowseResultCollection
            {
                BrowseResultOf(Array.Empty<byte>(), Reference("ns=2;s=B", "B", NodeClass.Object))
            });
            var browseCommand = new BrowseServer(AppId, null, _nodes.Add);

            // Act
            await _handler.HandleAsync(browseCommand);

            // Assert
            Assert.NotNull(_continued);
            Assert.Equal(ContinuationPoint, Assert.Single(_continued));
            Assert.Equal(TwoPagesNames, _nodes.Select(n => n.DisplayName));
        }

        /// <summary>
        /// Verifies that browse stops when BrowseNext returns no results.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WhenBrowseNextReturnsNoResults_StopsBrowsing()
        {
            // Arrange
            SetupBrowse(BrowseResultOf(ContinuationPoint, Reference("ns=2;s=A", "A", NodeClass.Object)));
            SetupBrowseNext(new BrowseResultCollection());
            var browseCommand = new BrowseServer(AppId, null, _nodes.Add);

            // Act
            await _handler.HandleAsync(browseCommand);

            // Assert
            Assert.Equal("A", Assert.Single(_nodes).DisplayName);
        }

        /// <summary>
        /// Verifies that no nodes are reported when browse returns no results.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WhenBrowseReturnsNoResults_ReportsNothing()
        {
            // Arrange
            SetupBrowse(null);
            var browseCommand = new BrowseServer(AppId, null, _nodes.Add);

            // Act
            await _handler.HandleAsync(browseCommand);

            // Assert
            Assert.Empty(_nodes);
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
