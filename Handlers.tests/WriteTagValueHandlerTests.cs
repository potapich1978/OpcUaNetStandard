using ChannelReader.Abstract;
using EventLogger;
using Events;
using Opc.Ua;
using Opc.Ua.Client;
using OpcSessions.Abstract;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the WriteTagValueHandler class.
    /// </summary>
    public class WriteTagValueHandlerTests
    {
        private const string AppId = "testApp";
        private const string UnknownAppId = "unknownApp";
        private const string TagId = "ns=2;s=Dev.Count";
        private const int Value = 42;
        private const double FractionalValue = 42.7;

        private readonly IGenericEventDispatcherLogger _logger;
        private readonly IOpcSessionsManager _sessions;
        private readonly ISession _session;
        private readonly WriteTagValueHandler _handler;

        /// <summary>
        /// Initializes test dependencies using mocks.
        /// </summary>
        public WriteTagValueHandlerTests()
        {
            _logger = Substitute.For<IGenericEventDispatcherLogger>();
            _sessions = Substitute.For<IOpcSessionsManager>();
            _session = Substitute.For<ISession>();
            _sessions.GetSession(AppId).Returns(_session);
            _session.ReadAsync(Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                    Arg.Any<ReadValueIdCollection>(), Arg.Any<CancellationToken>())
                .Returns(TagTypeOf(DataTypeIds.Int32));
            _handler = new WriteTagValueHandler(_logger, _sessions);
        }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnWriteTagValue()
        {
            // Act & Assert
            Assert.Equal(OpcCommandEvent.WriteTagValue, _handler.EventType);
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
        /// Verifies that HandleAsync logs an error and does not write when session is not registered.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithoutSession_LogsError()
        {
            // Arrange
            _sessions.GetSession(UnknownAppId).Returns((ISession)null);
            var writeCommand = new WriteTagValue(UnknownAppId, TagId, Value);

            // Act
            await _handler.HandleAsync(writeCommand);

            // Assert
            _logger.Received(1).LogError(Arg.Is<string>(s => s.Contains("not registered") && s.Contains(UnknownAppId)));
            await _session.DidNotReceive().WriteAsync(
                Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Verifies that the value is written to the tag.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithSupportedEvent_WritesValue()
        {
            // Arrange
            WriteValueCollection written = null;
            _session.WriteAsync(Arg.Any<RequestHeader>(),
                    Arg.Do<WriteValueCollection>(w => written = w), Arg.Any<CancellationToken>())
                .Returns(new WriteResponse { Results = new StatusCodeCollection { StatusCodes.Good } });
            var writeCommand = new WriteTagValue(AppId, TagId, Value);

            // Act
            await _handler.HandleAsync(writeCommand);

            // Assert
            Assert.NotNull(written);
            var writtenValue = Assert.Single(written);
            Assert.Equal(new NodeId(TagId), writtenValue.NodeId);
            Assert.Equal(new Variant(Value), writtenValue.Value.WrappedValue);
            _logger.DidNotReceive().LogError(Arg.Any<string>());
        }

        /// <summary>
        /// Verifies that a write rejected by the server is not swallowed and reaches the dispatcher.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WhenServerRejectsWrite_ThrowsToDispatcher()
        {
            // Arrange
            _session.WriteAsync(Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>())
                .Returns(new WriteResponse { Results = new StatusCodeCollection { StatusCodes.BadNotWritable } });
            var writeCommand = new WriteTagValue(AppId, TagId, Value);

            // Act
            var thrown = await Assert.ThrowsAsync<ServiceResultException>(() => _handler.HandleAsync(writeCommand));

            // Assert
            Assert.Equal(StatusCodes.BadNotWritable, thrown.StatusCode);
        }

        /// <summary>
        /// Verifies that a value that cannot be converted to the data type of the tag is not written
        /// and the failure reaches the dispatcher.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WhenValueCannotBeConverted_ThrowsToDispatcherAndDoesNotWrite()
        {
            // Arrange
            var writeCommand = new WriteTagValue(AppId, TagId, FractionalValue);

            // Act
            var thrown = await Assert.ThrowsAsync<ServiceResultException>(() => _handler.HandleAsync(writeCommand));

            // Assert
            Assert.Equal(StatusCodes.BadTypeMismatch, thrown.StatusCode);
            await _session.DidNotReceive().WriteAsync(
                Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>());
        }

        private static ReadResponse TagTypeOf(NodeId dataType)
            => new()
            {
                Results = new DataValueCollection
                {
                    new DataValue(new Variant(dataType)),
                    new DataValue(new Variant(ValueRanks.Scalar))
                }
            };
    }
}
