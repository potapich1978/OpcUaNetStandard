using ChannelReader.Abstract;
using EventLogger;
using Events;
using Opc.Ua.Client;
using OpcSessions.Abstract;
using OpcSubscriptions.Abstract;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the AddItemToSubscriptionHandler class.
    /// </summary>
    public class AddItemToSubscriptionHandlerTests
    {
        private readonly IGenericEventDispatcherLogger _logger;
        private readonly IOpcSubscriptions _subscriptions;
        private readonly IOpcSessionsManager _sessions;
        private readonly AddItemToSubscriptionHandler _handler;

        /// <summary>
        /// Initializes test dependencies using mocks.
        /// </summary>
        public AddItemToSubscriptionHandlerTests()
        {
            _logger = Substitute.For<IGenericEventDispatcherLogger>();
            _subscriptions = Substitute.For<IOpcSubscriptions>();
            _sessions = Substitute.For<IOpcSessionsManager>();
            _handler = new AddItemToSubscriptionHandler(_logger, _sessions, _subscriptions);
        }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnAddItemToSubscription()
        {
            // Act & Assert
            Assert.Equal(OpcCommandEvent.AddItemToSubscription, _handler.EventType);
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
        /// Verifies that HandleAsync logs an error when session is not found for the application.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithNonExistentSession_LogsError()
        {
            // Arrange
            var addCommand = new AddItemToSubscription("testApp", "testTag", "testKey", null);
            _sessions.GetSession(Arg.Any<string>()).Returns(default(ISession));

            // Act
            await _handler.HandleAsync(addCommand);

            // Assert
            _logger.Received(1).LogError(Arg.Is<string>(s => s.Contains("not registered")));
        }

        /// <summary>
        /// Verifies that HandleAsync calls AddSubscription when session exists and event is supported.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithValidSession_CallsAddSubscription()
        {
            // Arrange
            var session = Substitute.For<ISession>();
            var addCommand = new AddItemToSubscription("testApp", "testTag", "testKey", null);
            _sessions.GetSession(Arg.Any<string>()).Returns(session);

            // Act
            await _handler.HandleAsync(addCommand);

            // Assert
            await _subscriptions.Received(1).AddSubscriprion(
                session,
                addCommand.AppId,
                addCommand.TagId,
                addCommand.EventKey,
                addCommand.Callback);
        }
    }
}
