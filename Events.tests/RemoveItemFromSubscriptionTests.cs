namespace Events.tests
{
    /// <summary>
    /// Unit tests for the RemoveItemFromSubscription event class.
    /// </summary>
    public class RemoveItemFromSubscriptionTests
    {
        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnRemoveItemFromSubscription()
        {
            // Arrange
            var removeEvent = new RemoveItemFromSubscription("testApp", "testTag", "testKey");

            // Act & Assert
            Assert.Equal(OpcCommandEvent.RemoveItemFromSubscription, removeEvent.EventType);
        }

        /// <summary>
        /// Verifies that constructor correctly initializes all properties.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Act
            var removeEvent = new RemoveItemFromSubscription("testApp", "testTag", "testKey");

            // Assert
            Assert.Equal("testApp", removeEvent.AppId);
            Assert.Equal("testTag", removeEvent.TagId);
            Assert.Equal("testKey", removeEvent.EventKey);
        }
    }
}
