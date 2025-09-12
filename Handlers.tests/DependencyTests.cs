using EventChannelBuilder.Abstract;
using EventLogger;
using Events;
using Microsoft.Extensions.DependencyInjection;

namespace Handlers.tests
{
    /// <summary>
    /// Unit tests for the Dependency configuration class.
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
                .AddOpcBackend();

            // Act
            var manager = result.BuildServiceProvider().GetRequiredService<IChannelBuilder<OpcCommandEvent>>();

            // Assert
            Assert.Same(services, result);
            Assert.NotNull(manager);
        }
    }
}
