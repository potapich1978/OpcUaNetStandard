using ChannelReader.Abstract;
using EventLogger;
using Events;
using OpcSubscriptions.Abstract;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the RemoveItemFromSubscriptionHandler class.
    /// </summary>
    public class RemoveItemFromSubscriptionHandlerTests
    {
        private readonly IGenericEventDispatcherLogger _logger;
        private readonly IOpcSubscriptions _subscriptions;
        private readonly RemoveItemFromSubscriptionHandler _handler;

        /// <summary>
        /// Initializes test dependencies using mocks.
        /// </summary>
        public RemoveItemFromSubscriptionHandlerTests()
        {
            _logger = Substitute.For<IGenericEventDispatcherLogger>();
            _subscriptions = Substitute.For<IOpcSubscriptions>();
            _handler = new RemoveItemFromSubscriptionHandler(_logger, _subscriptions);
        }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnRemoveItemFromSubscription()
        {
            // Act & Assert
            Assert.Equal(OpcCommandEvent.RemoveItemFromSubscription, _handler.EventType);
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
        /// Verifies that HandleAsync calls RemoveItemFromSubscription with correct parameters when event is supported.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithSupportedEvent_CallsRemoveItemFromSubscription()
        {
            // Arrange
            var removeCommand = new Events.RemoveItemFromSubscription("testApp", "testTag", "testKey");

            // Act
            await _handler.HandleAsync(removeCommand);

            // Assert
            await _subscriptions.Received(1).RemoveItemFromSubscription(
                removeCommand.AppId,
                removeCommand.TagId,
                removeCommand.EventKey);
        }
    }

}
