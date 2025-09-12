using EventLogger;
using Microsoft.Extensions.DependencyInjection;
using OpcSessions.Abstract;

namespace OpcSessions.tests
{
    /// <summary>
    /// Tests for the Dependency configuration class in OpcSessions namespace.
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
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var result = services
                .AddSingleton(logger)
                .AddOpcSessionManager();

            // Act
            var manager = result.BuildServiceProvider().GetRequiredService<IOpcSessionsManager>();

            // Assert
            Assert.Same(services, result);
            Assert.NotNull(manager);
        }
    }
}
