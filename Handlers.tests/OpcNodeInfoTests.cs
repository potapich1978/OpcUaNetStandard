using Opc.Ua;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the OpcNodeInfo class.
    /// </summary>
    public class OpcNodeInfoTests
    {
        /// <summary>
        /// Verifies that an object reference is reported as a folder without data type.
        /// </summary>
        [Fact]
        public void Constructor_WithObjectReference_ReportsFolder()
        {
            // Arrange
            var description = Reference("ns=2;s=Device.Folder", "Folder", NodeClass.Object);

            // Act
            var info = new OpcNodeInfo(description, null);

            // Assert
            Assert.Equal("Folder", info.DisplayName);
            Assert.Equal("ns=2;s=Device.Folder", info.FullName);
            Assert.True(info.IsFolder);
            Assert.False(info.Writable);
            Assert.Null(info.DataType);
        }

        /// <summary>
        /// Verifies that a variable reference is reported as writable node with the given data type.
        /// </summary>
        [Fact]
        public void Constructor_WithVariableReference_ReportsWritableWithDataType()
        {
            // Arrange
            var description = Reference("ns=2;s=Device.Tag", "Tag", NodeClass.Variable);

            // Act
            var info = new OpcNodeInfo(description, typeof(float));

            // Assert
            Assert.Equal("Tag", info.DisplayName);
            Assert.Equal("ns=2;s=Device.Tag", info.FullName);
            Assert.False(info.IsFolder);
            Assert.True(info.Writable);
            Assert.Equal(typeof(float), info.DataType);
        }

        private static ReferenceDescription Reference(string nodeId, string displayName, NodeClass nodeClass)
            => new()
            {
                NodeId = new ExpandedNodeId(new NodeId(nodeId)),
                DisplayName = new LocalizedText(displayName),
                NodeClass = nodeClass
            };
    }
}
