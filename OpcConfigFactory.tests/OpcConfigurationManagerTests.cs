using Opc.Ua;
using OpcConfigFactory.Abstract;

namespace OpcConfigFactory.tests
{
    public class OpcConfigurationManagerTests
    {
        [Fact]
        public void Constructor_ShouldCreatePkiSubdirectories()
        {
            // Arrange
            var baseDir = AppContext.BaseDirectory;
            var pkiDir = Path.Combine(baseDir, "pki");

            // Act
            var manager = new OpcConfigurationManager();

            // Assert
            Assert.True(Directory.Exists(Path.Combine(pkiDir, "own")));
            Assert.True(Directory.Exists(Path.Combine(pkiDir, "trusted")));
            Assert.True(Directory.Exists(Path.Combine(pkiDir, "issuers")));
            Assert.True(Directory.Exists(Path.Combine(pkiDir, "rejected")));
        }

        [Fact]
        public async Task ProduceConfig_ShouldReturnConfigWithCorrectValues()
        {
            // Arrange
            var manager = new OpcConfigurationManager();
            var transport = Substitute.For<ITransportParameters>();

            transport.OperationTimeoutSec.Returns(15);
            transport.MaxMessageSizeBytes.Returns(4096);
            transport.MaxBufferSizeBytes.Returns(2048);
            transport.ChannelLifeTimeSec.Returns(60);
            transport.SecurityTokenLifeTimeSec.Returns(120);
            transport.SessionTimeoutSec.Returns(300);
            transport.MinSubscriptionLifeTimeSec.Returns(30);

            // Act
            var config = await manager.ProduceConfig("TestApp", transport);

            // Assert
            Assert.Equal("TestApp", config.ApplicationName);
            Assert.Equal(ApplicationType.Client, config.ApplicationType);

            Assert.Equal(15000, config.TransportQuotas.OperationTimeout);
            Assert.Equal(4096, config.TransportQuotas.MaxMessageSize);
            Assert.Equal(2048, config.TransportQuotas.MaxBufferSize);
            Assert.Equal(60000, config.TransportQuotas.ChannelLifetime);
            Assert.Equal(120000, config.TransportQuotas.SecurityTokenLifetime);

            Assert.Equal(300000, config.ClientConfiguration.DefaultSessionTimeout);
            Assert.Equal(30000, config.ClientConfiguration.MinSubscriptionLifetime);

            Assert.True(config.SecurityConfiguration.AutoAcceptUntrustedCertificates);
        }

        [Fact]
        public async Task ProduceConfig_ShouldCacheConfiguration()
        {
            // Arrange
            var manager = new OpcConfigurationManager();
            var transport = Substitute.For<ITransportParameters>();
            transport.OperationTimeoutSec.Returns(1);
            transport.MaxMessageSizeBytes.Returns(100);
            transport.MaxBufferSizeBytes.Returns(50);
            transport.ChannelLifeTimeSec.Returns(10);
            transport.SecurityTokenLifeTimeSec.Returns(20);
            transport.SessionTimeoutSec.Returns(30);
            transport.MinSubscriptionLifeTimeSec.Returns(5);

            // Act
            var config1 = await manager.ProduceConfig("CacheApp", transport);
            var config2 = await manager.ProduceConfig("CacheApp", transport);

            // Assert
            Assert.Same(config1, config2); // оба указывают на один и тот же объект из кэша
        }

        [Fact]
        public async Task ProduceConfig_DifferentAppNames_ShouldReturnDifferentInstances()
        {
            // Arrange
            var manager = new OpcConfigurationManager();
            var transport = Substitute.For<ITransportParameters>();
            transport.OperationTimeoutSec.Returns(10);
            transport.MaxMessageSizeBytes.Returns(1000);
            transport.MaxBufferSizeBytes.Returns(500);
            transport.ChannelLifeTimeSec.Returns(20);
            transport.SecurityTokenLifeTimeSec.Returns(40);
            transport.SessionTimeoutSec.Returns(60);
            transport.MinSubscriptionLifeTimeSec.Returns(15);

            // Act
            var config1 = await manager.ProduceConfig("AppOne", transport);
            var config2 = await manager.ProduceConfig("AppTwo", transport);

            // Assert
            Assert.NotSame(config1, config2);
            Assert.Equal("AppOne", config1.ApplicationName);
            Assert.Equal("AppTwo", config2.ApplicationName);
        }
    }
}
