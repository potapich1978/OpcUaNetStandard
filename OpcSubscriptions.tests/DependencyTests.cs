using Microsoft.Extensions.DependencyInjection;
using OpcSubscriptions.Abstract;

namespace OpcSubscriptions.tests
{
    /// <summary>
    /// Tests for the Dependency configuration class in OpcSubscriptions namespace.
    /// </summary>
    public class DependencyTests
    {
        /// <summary>
        /// Verifies that AddOpcSessionManager registers IOpcSessionsManager service
        /// with OpcSessionsManager implementation
        /// </summary>
        [Fact]
        public void AddOpcSessionsManager_RegistersOpcSessionsManager()
        {
            // Arrange
            var services = new ServiceCollection();
            var result = services.AddSubscriptionManager();

            // Act
            var manager = result.BuildServiceProvider().GetRequiredService<IOpcSubscriptions>();

            // Assert
            Assert.Same(services, result);
            Assert.NotNull(manager);
        }
    }
}
