using ChannelReader.Abstract;
using EventLogger;
using Events;
using Opc.Ua;
using Opc.Ua.Client;
using OpcSessions.Abstract;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the WriteTagValueCompleteHandler class.
    /// </summary>
    public class WriteTagValueCompleteHandlerTests
    {
        private const string AppId = "testApp";
        private const string UnknownAppId = "unknownApp";
        private const string TagId = "ns=2;s=Dev.Count";
        private const int Value = 42;
        private const double FractionalValue = 42.7;

        private readonly IGenericEventDispatcherLogger _logger;
        private readonly IOpcSessionsManager _sessions;
        private readonly ISession _session;
        private readonly WriteTagValueCompleteHandler _handler;
        private readonly List<Exception> _completions = new();

        /// <summary>
        /// Initializes test dependencies using mocks.
        /// </summary>
        public WriteTagValueCompleteHandlerTests()
        {
            _logger = Substitute.For<IGenericEventDispatcherLogger>();
            _sessions = Substitute.For<IOpcSessionsManager>();
            _session = Substitute.For<ISession>();
            _sessions.GetSession(AppId).Returns(_session);
            _session.ReadAsync(Arg.Any<RequestHeader>(), Arg.Any<double>(), Arg.Any<TimestampsToReturn>(),
                    Arg.Any<ReadValueIdCollection>(), Arg.Any<CancellationToken>())
                .Returns(TagTypeOf(DataTypeIds.Int32));
            _handler = new WriteTagValueCompleteHandler(_logger, _sessions);
        }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnWriteTagValueComplete()
        {
            // Act & Assert
            Assert.Equal(OpcCommandEvent.WriteTagValueComplete, _handler.EventType);
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
            var writeCommand = new WriteTagValueComplete(UnknownAppId, TagId, Value, Complete);

            // Act
            await _handler.HandleAsync(writeCommand);

            // Assert
            _logger.Received(1).LogError(Arg.Is<string>(s => s.Contains("not registered")));
            var error = Assert.IsType<InvalidOperationException>(Assert.Single(_completions));
            Assert.Contains(UnknownAppId, error.Message);
            await _session.DidNotReceive().WriteAsync(
                Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Verifies that the value is written to the tag and the callback is invoked once without error.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithSupportedEvent_WritesValueAndCompletesWithoutError()
        {
            // Arrange
            WriteValueCollection written = null;
            _session.WriteAsync(Arg.Any<RequestHeader>(),
                    Arg.Do<WriteValueCollection>(w => written = w), Arg.Any<CancellationToken>())
                .Returns(WriteResponseOf(StatusCodes.Good));
            var writeCommand = new WriteTagValueComplete(AppId, TagId, Value, Complete);

            // Act
            await _handler.HandleAsync(writeCommand);

            // Assert
            Assert.NotNull(written);
            var writtenValue = Assert.Single(written);
            Assert.Equal(new NodeId(TagId), writtenValue.NodeId);
            Assert.Equal(new Variant(Value), writtenValue.Value.WrappedValue);
            Assert.Null(Assert.Single(_completions));
            _logger.DidNotReceive().LogError(Arg.Any<string>());
        }

        /// <summary>
        /// Verifies that a write rejected by the server is delivered to the callback with the server status
        /// and then rethrown to the dispatcher.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WhenServerRejectsWrite_CompletesWithServerStatusAndRethrows()
        {
            // Arrange
            _session.WriteAsync(Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>())
                .Returns(WriteResponseOf(StatusCodes.BadNotWritable));
            var writeCommand = new WriteTagValueComplete(AppId, TagId, Value, Complete);

            // Act
            var thrown = await Assert.ThrowsAsync<ServiceResultException>(() => _handler.HandleAsync(writeCommand));

            // Assert
            Assert.Equal(StatusCodes.BadNotWritable, thrown.StatusCode);
            Assert.Same(thrown, Assert.Single(_completions));
        }

        /// <summary>
        /// Verifies that a write failure is delivered to the callback and then rethrown to the dispatcher.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WhenWriteFails_CompletesWithErrorAndRethrows()
        {
            // Arrange
            var failure = new ServiceResultException(StatusCodes.BadConnectionClosed);
            _session.WriteAsync(Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>())
                .Returns<WriteResponse>(_ => throw failure);
            var writeCommand = new WriteTagValueComplete(AppId, TagId, Value, Complete);

            // Act
            var thrown = await Assert.ThrowsAsync<ServiceResultException>(() => _handler.HandleAsync(writeCommand));

            // Assert
            Assert.Same(failure, thrown);
            Assert.Same(failure, Assert.Single(_completions));
        }

        /// <summary>
        /// Verifies that a value that cannot be converted to the data type of the tag is not written,
        /// is delivered to the callback as the error and then rethrown to the dispatcher.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WhenValueCannotBeConverted_CompletesWithErrorAndDoesNotWrite()
        {
            // Arrange
            var writeCommand = new WriteTagValueComplete(AppId, TagId, FractionalValue, Complete);

            // Act
            var thrown = await Assert.ThrowsAsync<ServiceResultException>(() => _handler.HandleAsync(writeCommand));

            // Assert
            Assert.Equal(StatusCodes.BadTypeMismatch, thrown.StatusCode);
            Assert.Same(thrown, Assert.Single(_completions));
            await _session.DidNotReceive().WriteAsync(
                Arg.Any<RequestHeader>(), Arg.Any<WriteValueCollection>(), Arg.Any<CancellationToken>());
        }

        private void Complete(Exception error)
            => _completions.Add(error);

        private static ReadResponse TagTypeOf(NodeId dataType)
            => new()
            {
                Results = new DataValueCollection
                {
                    new DataValue(new Variant(dataType)),
                    new DataValue(new Variant(ValueRanks.Scalar))
                }
            };

        private static WriteResponse WriteResponseOf(uint status)
            => new() { Results = new StatusCodeCollection { status } };
    }
}
