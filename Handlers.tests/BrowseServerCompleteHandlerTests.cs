using ChannelReader.Abstract;
using EventLogger;
using Events;
using Opc.Ua;
using Opc.Ua.Client;
using OpcSessions.Abstract;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the BrowseServerCompleteHandler class.
    /// </summary>
    public class BrowseServerCompleteHandlerTests
    {
        private const string AppId = "testApp";
        private const string UnknownAppId = "unknownApp";
        private static readonly string[] BrowsedNames = { "Folder", "Tag" };

        private readonly IGenericEventDispatcherLogger _logger;
        private readonly IOpcSessionsManager _sessions;
        private readonly ISession _session;
        private readonly BrowseServerCompleteHandler _handler;
        private readonly List<(IReadOnlyList<IOpcNodeInfo> Nodes, Exception Error)> _completions = new();

        /// <summary>
        /// Initializes test dependencies using mocks.
        /// </summary>
        public BrowseServerCompleteHandlerTests()
        {
            _logger = Substitute.For<IGenericEventDispatcherLogger>();
            _sessions = Substitute.For<IOpcSessionsManager>();
            _session = Substitute.For<ISession>();
            _sessions.GetSession(AppId).Returns(_session);
            _handler = new BrowseServerCompleteHandler(_logger, _sessions);
        }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnBrowseComplete()
        {
            // Act & Assert
            Assert.Equal(OpcCommandEvent.BrowseComplete, _handler.EventType);
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
        /// Verifies that a missing session is logged and delivered to the callback as an error.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithoutSession_LogsErrorAndCompletesWithError()
        {
            // Arrange
            _sessions.GetSession(UnknownAppId).Returns((ISession)null);
            var browseCommand = new BrowseServerComplete(UnknownAppId, null, Complete);

            // Act
            await _handler.HandleAsync(browseCommand);

            // Assert
            _logger.Received(1).LogError(Arg.Is<string>(s => s.Contains("not registered")));
            var completion = Assert.Single(_completions);
            Assert.Null(completion.Nodes);
            var error = Assert.IsType<InvalidOperationException>(completion.Error);
            Assert.Contains(UnknownAppId, error.Message);
            await _session.DidNotReceive().BrowseAsync(
                Arg.Any<RequestHeader>(), Arg.Any<ViewDescription>(), Arg.Any<uint>(),
                Arg.Any<BrowseDescriptionCollection>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Verifies that the whole browsed level is delivered to the callback once without error.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithSupportedEvent_CompletesWithAllNodes()
        {
            // Arrange
            BrowseDescriptionCollection browsed = null;
            _session.BrowseAsync(Arg.Any<RequestHeader>(), Arg.Any<ViewDescription>(), Arg.Any<uint>(),
                    Arg.Do<BrowseDescriptionCollection>(b => browsed = b), Arg.Any<CancellationToken>())
                .Returns(new BrowseResponse
                {
                    Results = new BrowseResultCollection
                    {
                        new BrowseResult
                        {
                            References = new ReferenceDescriptionCollection
                            {
                                Reference("ns=2;s=Dev.Folder", "Folder", NodeClass.Object),
                                Reference("ns=2;s=Dev.Tag", "Tag", NodeClass.Variable)
                            }
                        }
                    }
                });
            _session.ReadAsync(Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                    Arg.Any<ReadValueIdCollection>(), Arg.Any<CancellationToken>())
                .Returns(new ReadResponse
                {
                    Results = new DataValueCollection
                    {
                        new DataValue(new Variant(DataTypeIds.Float)),
                        new DataValue(new Variant(ValueRanks.Scalar))
                    }
                });
            var browseCommand = new BrowseServerComplete(AppId, "ns=2;s=Dev", Complete);

            // Act
            await _handler.HandleAsync(browseCommand);

            // Assert
            Assert.NotNull(browsed);
            Assert.Equal(new NodeId("ns=2;s=Dev"), Assert.Single(browsed).NodeId);
            var completion = Assert.Single(_completions);
            Assert.Null(completion.Error);
            Assert.Equal(BrowsedNames, completion.Nodes.Select(n => n.DisplayName));
            Assert.Equal(typeof(float), completion.Nodes[1].DataType);
            _logger.DidNotReceive().LogError(Arg.Any<string>());
        }

        /// <summary>
        /// Verifies that a browse failure is delivered to the callback and then rethrown to the dispatcher.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WhenBrowseFails_CompletesWithErrorAndRethrows()
        {
            // Arrange
            var failure = new ServiceResultException(StatusCodes.BadConnectionClosed);
            _session.BrowseAsync(Arg.Any<RequestHeader>(), Arg.Any<ViewDescription>(), Arg.Any<uint>(),
                    Arg.Any<BrowseDescriptionCollection>(), Arg.Any<CancellationToken>())
                .Returns<BrowseResponse>(_ => throw failure);
            var browseCommand = new BrowseServerComplete(AppId, null, Complete);

            // Act
            var thrown = await Assert.ThrowsAsync<ServiceResultException>(() => _handler.HandleAsync(browseCommand));

            // Assert
            Assert.Same(failure, thrown);
            var completion = Assert.Single(_completions);
            Assert.Null(completion.Nodes);
            Assert.Same(failure, completion.Error);
        }

        private void Complete(IReadOnlyList<IOpcNodeInfo> nodes, Exception error)
            => _completions.Add((nodes, error));

        private static ReferenceDescription Reference(string nodeId, string displayName, NodeClass nodeClass)
            => new()
            {
                NodeId = new ExpandedNodeId(new NodeId(nodeId)),
                DisplayName = new LocalizedText(displayName),
                NodeClass = nodeClass
            };
    }
}
