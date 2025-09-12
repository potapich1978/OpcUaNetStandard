using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSessions.Abstract;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the RegisterSessionHandler class.
    /// </summary>
    public class RegisterSessionHandlerTests
    {
        private readonly IGenericEventDispatcherLogger _logger;
        private readonly IOpcSessionsManager _sessions;
        private readonly RegisterSessionHandler _handler;

        /// <summary>
        /// Initializes test dependencies using mocks.
        /// </summary>
        public RegisterSessionHandlerTests()
        {
            _logger = Substitute.For<IGenericEventDispatcherLogger>();
            _sessions = Substitute.For<IOpcSessionsManager>();
            _handler = new RegisterSessionHandler(_sessions, _logger);
        }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnRegisterSession()
        {
            // Act & Assert
            Assert.Equal(OpcCommandEvent.RegisterSession, _handler.EventType);
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
        }

        /// <summary>
        /// Verifies that HandleAsync calls AddSession with correct parameters when event is supported.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithSupportedEvent_CallsAddSessionWithCorrectParams()
        {
            // Arrange
            var registerCommand = new RegisterSession(
                "testApp",
                "testEndpoint",
                300, 10, 65536, 4194304, 10, 5, 600, 60);

            // Act
            await _handler.HandleAsync(registerCommand);

            // Assert
            await _sessions.Received(1).AddSession(Arg.Is<ISessionChannelParams>(p =>
                p.AppId == registerCommand.AppId &&
                p.ServerEndPoint == registerCommand.Endpoint &&
                p.ChannelLifeTimeSec == registerCommand.ChannelLifeTimeSec &&
                p.KeepAliveIntervalSec == registerCommand.KeepAliveIntervalSec &&
                p.MaxBufferSizeBytes == registerCommand.MaxBufferSizeBytes &&
                p.MaxMessageSizeBytes == registerCommand.MaxMessageSizeBytes &&
                p.MinSubscriptionLifeTimeSec == registerCommand.MinSubscriptionLifeTimeSec &&
                p.OperationTimeoutSec == registerCommand.OperationTimeoutSec &&
                p.SecurityTokenLifeTimeSec == registerCommand.SecurityTokenLifeTimeSec &&
                p.SessionTimeoutSec == registerCommand.SessionTimeoutSec));
        }
    }
}
