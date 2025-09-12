using NSubstitute;
using Opc.Ua.Client;

namespace Events.tests
{
    /// <summary>
    /// Unit tests for the AddItemToSubscription event class.
    /// </summary>
    public class AddItemToSubscriptionTests
    {
        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnAddItemToSubscription()
        {
            // Arrange
            var callback = Substitute.For<MonitoredItemNotificationEventHandler>();
            var addEvent = new AddItemToSubscription("testApp", "testTag", "testKey", callback);

            // Act & Assert
            Assert.Equal(OpcCommandEvent.AddItemToSubscription, addEvent.EventType);
        }

        /// <summary>
        /// Verifies that constructor correctly initializes all properties.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Arrange
            var callback = Substitute.For<MonitoredItemNotificationEventHandler>();

            // Act
            var addEvent = new AddItemToSubscription("testApp", "testTag", "testKey", callback);

            // Assert
            Assert.Equal("testApp", addEvent.AppId);
            Assert.Equal("testTag", addEvent.TagId);
            Assert.Equal("testKey", addEvent.EventKey);
            Assert.Equal(callback, addEvent.Callback);
        }
    }
}
