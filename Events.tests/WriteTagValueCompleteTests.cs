namespace Events.tests
{
    /// <summary>
    /// Unit tests for the WriteTagValueComplete event class.
    /// </summary>
    public class WriteTagValueCompleteTests
    {
        private const string AppId = "testApp";
        private const string TagId = "ns=2;s=Dev.Count";

        private static void Completed(Exception? error) { }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnWriteTagValueComplete()
        {
            // Arrange
            var writeEvent = new WriteTagValueComplete(AppId, TagId, 42, Completed);

            // Act & Assert
            Assert.Equal(OpcCommandEvent.WriteTagValueComplete, writeEvent.EventType);
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
            var writeEvent = new WriteTagValueComplete(AppId, TagId, value, Completed);

            // Assert
            Assert.Equal(AppId, writeEvent.AppId);
            Assert.Equal(TagId, writeEvent.TagId);
            Assert.Same(value, writeEvent.Value);
            Assert.Equal(Completed, writeEvent.Completed);
        }

        /// <summary>
        /// Verifies that constructor rejects a null completion callback.
        /// </summary>
        [Fact]
        public void Constructor_WithNullCompleted_Throws()
        {
            // Act & Assert
            var error = Assert.Throws<ArgumentNullException>(
                () => new WriteTagValueComplete(AppId, TagId, 42, null!));
            Assert.Equal("completed", error.ParamName);
        }
    }
}
