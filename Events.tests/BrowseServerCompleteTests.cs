namespace Events.tests
{
    /// <summary>
    /// Unit tests for the BrowseServerComplete event class.
    /// </summary>
    public class BrowseServerCompleteTests
    {
        private const string AppId = "testApp";

        private static void Completed(IReadOnlyList<IOpcNodeInfo> nodes, Exception? error) { }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnBrowseComplete()
        {
            // Arrange
            var browseEvent = new BrowseServerComplete(AppId, null, Completed);

            // Act & Assert
            Assert.Equal(OpcCommandEvent.BrowseComplete, browseEvent.EventType);
        }

        /// <summary>
        /// Verifies that constructor correctly initializes all properties.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Act
            var browseEvent = new BrowseServerComplete(AppId, "ns=2;s=Folder", Completed);

            // Assert
            Assert.Equal(AppId, browseEvent.AppId);
            Assert.Equal("ns=2;s=Folder", browseEvent.NodeFullName);
            Assert.Equal(Completed, browseEvent.Completed);
        }

        /// <summary>
        /// Verifies that constructor rejects a null completion callback.
        /// </summary>
        [Fact]
        public void Constructor_WithNullCompleted_Throws()
        {
            // Act & Assert
            var error = Assert.Throws<ArgumentNullException>(() => new BrowseServerComplete(AppId, null, null!));
            Assert.Equal("completed", error.ParamName);
        }
    }
}
