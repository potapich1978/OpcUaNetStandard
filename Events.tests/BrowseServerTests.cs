namespace Events.tests
{
    /// <summary>
    /// Unit tests for the BrowseServer event class.
    /// </summary>
    public class BrowseServerTests
    {
        private const string AppId = "testApp";

        private static void NodeAction(IOpcNodeInfo node) { }

        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnBrowse()
        {
            // Arrange
            var browseEvent = new BrowseServer(AppId, null, NodeAction);

            // Act & Assert
            Assert.Equal(OpcCommandEvent.Browse, browseEvent.EventType);
        }

        /// <summary>
        /// Verifies that constructor correctly initializes all properties.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Act
            var browseEvent = new BrowseServer(AppId, "ns=2;s=Folder", NodeAction);

            // Assert
            Assert.Equal(AppId, browseEvent.AppId);
            Assert.Equal("ns=2;s=Folder", browseEvent.NodeFullName);
            Assert.Equal(NodeAction, browseEvent.NodeAction);
        }

        /// <summary>
        /// Verifies that constructor rejects a null node action.
        /// </summary>
        [Fact]
        public void Constructor_WithNullNodeAction_Throws()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BrowseServer(AppId, null, null!));
        }
    }
}
