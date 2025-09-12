namespace Events.tests
{
    /// <summary>
    /// Unit tests for the RegisterSession event class.
    /// </summary>
    public class RegisterSessionTests
    {
        /// <summary>
        /// Verifies that the EventType property returns the correct OPC command event type.
        /// </summary>
        [Fact]
        public void EventType_ShouldReturnRegisterSession()
        {
            // Arrange
            var registerEvent = new RegisterSession("testApp", "testEndpoint");

            // Act & Assert
            Assert.Equal(OpcCommandEvent.RegisterSession, registerEvent.EventType);
        }

        /// <summary>
        /// Verifies that constructor with minimal parameters correctly initializes properties with default values.
        /// </summary>
        [Fact]
        public void Constructor_WithMinimalParameters_ShouldInitializeWithDefaults()
        {
            // Act
            var registerEvent = new RegisterSession("testApp", "testEndpoint");

            // Assert
            Assert.Equal("testApp", registerEvent.AppId);
            Assert.Equal("testEndpoint", registerEvent.Endpoint);
            Assert.Equal(300, registerEvent.ChannelLifeTimeSec);
            Assert.Equal(10, registerEvent.KeepAliveIntervalSec);
            Assert.Equal(65536, registerEvent.MaxBufferSizeBytes);
            Assert.Equal(4194304, registerEvent.MaxMessageSizeBytes);
            Assert.Equal(10, registerEvent.MinSubscriptionLifeTimeSec);
            Assert.Equal(5, registerEvent.OperationTimeoutSec);
            Assert.Equal(600, registerEvent.SecurityTokenLifeTimeSec);
            Assert.Equal(60, registerEvent.SessionTimeoutSec);
        }

        /// <summary>
        /// Verifies that constructor with all parameters correctly initializes all properties.
        /// </summary>
        [Fact]
        public void Constructor_WithAllParameters_ShouldInitializeAllProperties()
        {
            // Act
            var registerEvent = new RegisterSession(
                "testApp", "testEndpoint", 400, 15, 32768,
                2097152, 20, 10, 1200, 120);

            // Assert
            Assert.Equal("testApp", registerEvent.AppId);
            Assert.Equal("testEndpoint", registerEvent.Endpoint);
            Assert.Equal(400, registerEvent.ChannelLifeTimeSec);
            Assert.Equal(15, registerEvent.KeepAliveIntervalSec);
            Assert.Equal(32768, registerEvent.MaxBufferSizeBytes);
            Assert.Equal(2097152, registerEvent.MaxMessageSizeBytes);
            Assert.Equal(20, registerEvent.MinSubscriptionLifeTimeSec);
            Assert.Equal(10, registerEvent.OperationTimeoutSec);
            Assert.Equal(1200, registerEvent.SecurityTokenLifeTimeSec);
            Assert.Equal(120, registerEvent.SessionTimeoutSec);
        }
    }
}
