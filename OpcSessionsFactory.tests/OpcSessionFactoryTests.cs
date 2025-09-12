using EventLogger;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OpcConfigFactory.Abstract;
using OpcSessionFactory.Abstract;

namespace OpcSessionsFactory.tests
{
    /// <summary>
    /// Unit tests for <see cref="OpcSessionFactory"/>.
    /// Covers behavior that can be verified without real OPC UA server integration.
    /// </summary>
    public class OpcSessionFactoryTests
    {
        private readonly IOpcConfigurationManager _configManagerMock;
        private readonly IGenericEventDispatcherLogger _loggerMock;
        private readonly IOpcFoundationSession _opcFoundationWrapper;
        private readonly OpcSessionFactory.OpcSessionFactory _factory;

        public OpcSessionFactoryTests()
        {
            _configManagerMock = Substitute.For<IOpcConfigurationManager>();
            _loggerMock = Substitute.For<IGenericEventDispatcherLogger>();
            _opcFoundationWrapper = Substitute.For<IOpcFoundationSession>();
            _factory = new OpcSessionFactory.OpcSessionFactory(_configManagerMock, _opcFoundationWrapper, _loggerMock);
        }

        /// <summary>
        /// Ensures that <see cref="OpcSessionFactory"/> passes transport parameters
        /// from <see cref="ISessionParams"/> to the configuration manager.
        /// </summary>
        [Fact]
        public async Task GetSession_ShouldCallProduceConfig_WithCorrectParameters()
        {
            // Arrange
            var sessionParams = Substitute.For<ISessionParams>();
            sessionParams.AppId.Returns("TestApp");
            sessionParams.ServerEndPoint.Returns("opc.tcp://localhost:4840");
            sessionParams.ChannelLifeTimeSec.Returns(100);
            sessionParams.SessionTimeoutSec.Returns(200);
            sessionParams.OperationTimeoutSec.Returns(300);
            sessionParams.SecurityTokenLifeTimeSec.Returns(400);
            sessionParams.KeepAliveIntervalSec.Returns(500);
            sessionParams.MaxBufferSizeBytes.Returns(600);
            sessionParams.MaxMessageSizeBytes.Returns(700);
            sessionParams.MinSubscriptionLifeTimeSec.Returns(800);

            var config = new ApplicationConfiguration();
            _configManagerMock
                .ProduceConfig("TestApp", Arg.Any<ITransportParameters>())
                .Returns(Task.FromResult(config));

            var endpoint = new EndpointDescription("opc.tcp://localhost:4840");
            var fakeSession = Substitute.For<ISession>();
            _opcFoundationWrapper.ValidatAppInstance(Arg.Any<ApplicationInstance>()).Returns(Task.FromResult(true));
            _opcFoundationWrapper.SelectEndpoint(Arg.Any<ApplicationConfiguration>(), Arg.Any<string>(), 
                                                 Arg.Any<CancellationToken>()).Returns(endpoint);

            _opcFoundationWrapper.ProduceBaseOpcSession(
                                Arg.Any<ApplicationConfiguration>(),
                                Arg.Any<ConfiguredEndpoint>(),
                                Arg.Any<bool>(),
                                Arg.Any<string>(),
                                Arg.Any<uint>(),
                                Arg.Any<IUserIdentity>(),
                                Arg.Any<IList<string>>())
                            .Returns(Task.FromResult(fakeSession));

            // Act
            var result = await _factory.GetSession(sessionParams);

            // Assert
            await _configManagerMock.Received(1).ProduceConfig(
                "TestApp",
                Arg.Is<ITransportParameters>(p =>
                    p.ChannelLifeTimeSec == 100 &&
                    p.SessionTimeoutSec == 200 &&
                    p.OperationTimeoutSec == 300 &&
                    p.SecurityTokenLifeTimeSec == 400 &&
                    p.KeepAliveIntervalSec == 500 &&
                    p.MaxBufferSizeBytes == 600 &&
                    p.MaxMessageSizeBytes == 700 &&
                    p.MinSubscriptionLifeTimeSec == 800
                ));

            Assert.IsAssignableFrom<IOpcUaSession>(result);
            Assert.False(result.Session.DeleteSubscriptionsOnClose);
            Assert.True(result.Session.TransferSubscriptionsOnReconnect);
            Assert.Equal(sessionParams.KeepAliveIntervalSec * 1000, result.Session.KeepAliveInterval);
        }
    }
}
