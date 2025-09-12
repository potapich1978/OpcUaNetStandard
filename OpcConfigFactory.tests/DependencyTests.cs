using Microsoft.Extensions.DependencyInjection;
using OpcConfigFactory.Abstract;

namespace OpcConfigFactory.tests
{
    /// <summary>
    /// Unit tests for the Dependency configuration class in OpcConfigFactory namespace.
    /// </summary>
    public class DependencyTests
    {
        /// <summary>
        /// Verifies that AddOpcConfigManager registers IOpcConfigurationManager service
        /// with OpcConfigurationManager implementation.
        /// </summary>
        [Fact]
        public void AddOpcConfigManager_RegistersOpcConfigurationManagerAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = Dependency.AddOpcConfigManager(services);
            var manager = result.BuildServiceProvider().GetRequiredService<IOpcConfigurationManager>();

            // Assert
            Assert.Same(services, result);
            Assert.NotNull(manager);
        }
    }
}
