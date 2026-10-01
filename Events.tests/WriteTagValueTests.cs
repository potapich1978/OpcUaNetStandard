namespace Events.tests
{
    /// <summary>
    /// Unit tests for the WriteTagValue event class.
    /// </summary>
    public class WriteTagValueTests
    {
        private const string AppId = "testApp";
        private const string TagId = "ns=2;s=Dev.Count";

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnWriteTagValue()
        {
            // Arrange
            var writeEvent = new WriteTagValue(AppId, TagId, 42);

            // Act & Assert
            Assert.Equal(OpcCommandEvent.WriteTagValue, writeEvent.EventType);
        }

        /// <summary>
        /// Verifies that constructor correctly initializes all properties.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Arrange
            object value = (ushort)42;

            // Act
            var writeEvent = new WriteTagValue(AppId, TagId, value);

            // Assert
            Assert.Equal(AppId, writeEvent.AppId);
            Assert.Equal(TagId, writeEvent.TagId);
            Assert.Same(value, writeEvent.Value);
        }
    }
}
