using EventLogger;
using Microsoft.Extensions.DependencyInjection;
using OpcSessionFactory;
using OpcSessionFactory.Abstract;

namespace OpcSessionsFactory.tests
{
    /// <summary>
    /// Tests for the Dependency configuration class in OpcSessionFactory namespace.
    /// </summary>
    public class DependencyTests
    {
        /// <summary>
        /// Verifies that AddOpcSessionManager registers IOpcSessionFactory service
        /// with OpcSessionFactory implementation
        /// </summary>
        [Fact]
        public void AddOpcSessionsFactory_RegistersOpcSessionsFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var result = services
                .AddSingleton(logger)
                .AddOpcSessionFactory();

            // Act
            var manager = result.BuildServiceProvider().GetRequiredService<IOpcSessionFactory>();

            // Assert
            Assert.Same(services, result);
            Assert.NotNull(manager);
        }
    }
}
