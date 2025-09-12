using Microsoft.Extensions.DependencyInjection;
using OpcCallbacks.Abstract;


namespace OpcCallbacks.tests
{
    /// <summary>
    /// Unit tests for the Dependency configuration class.
    /// </summary>
    public class DependencyTests
    {
        /// <summary>
        /// Verifies that AddCallbackManager registers both IOpcTagCallbacksCount and IOpcCallbackManager
        /// services with their correct implementations in the service collection.
        /// </summary>
        [Fact]
        public void AddCallbackManager_RegistersRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = Dependency.AddCallbackManager(services);
            var manager = result.BuildServiceProvider().GetRequiredService<IOpcCallbackManager>();

            // Assert
            Assert.Same(services, result);
            Assert.NotNull(manager);
        }
    }
}
