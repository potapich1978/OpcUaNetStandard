using EventLogger;
using Opc.Ua.Client;
using OpcSessionFactory.Abstract;
using OpcSessions.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcSessions.tests
{
    /// <summary>
    /// Unit tests for <see cref="OpcSessionsManager"/>.
    /// Verifies session lifecycle management without real OPC UA integration.
    /// </summary>
    public class OpcSessionsManagerTests
    {
        private readonly IOpcSessionFactory _factoryMock;
        private readonly IGenericEventDispatcherLogger _loggerMock;
        private readonly OpcSessionsManager _manager;

        public OpcSessionsManagerTests()
        {
            _factoryMock = Substitute.For<IOpcSessionFactory>();
            _loggerMock = Substitute.For<IGenericEventDispatcherLogger>();
            _manager = new OpcSessionsManager(_factoryMock, _loggerMock);
        }

        /// <summary>
        /// Ensures that <see cref="OpcSessionsManager.AddSession"/> 
        /// creates a new session using the factory and stores it in the dictionary.
        /// </summary>
        [Fact]
        public async Task AddSession_ShouldCreateAndStoreSession()
        {
            // Arrange
            var sessionParams = Substitute.For<ISessionChannelParams>();
            sessionParams.AppId.Returns("App1");
            sessionParams.ServerEndPoint.Returns("opc.tcp://localhost:4840");
            sessionParams.ChannelLifeTimeSec.Returns(111);
            sessionParams.KeepAliveIntervalSec.Returns(222);
            sessionParams.MaxBufferSizeBytes.Returns(333);
            sessionParams.MaxMessageSizeBytes.Returns(444);
            sessionParams.MinSubscriptionLifeTimeSec.Returns(555);
            sessionParams.OperationTimeoutSec.Returns(666);
            sessionParams.SecurityTokenLifeTimeSec.Returns(777);
            sessionParams.SessionTimeoutSec.Returns(888);

            var sessionMock = Substitute.For<IOpcUaSession>();
            var uaSessionMock = Substitute.For<ISession>();
            sessionMock.Session.Returns(uaSessionMock);

            _factoryMock.GetSession(Arg.Any<ISessionParams>())
                        .Returns(Task.FromResult(sessionMock));

            // Act
            await _manager.AddSession(sessionParams);

            // Assert
            await _factoryMock.Received(1).GetSession(
                Arg.Is<ISessionParams>(p =>
                    p.AppId == "App1" &&
                    p.ServerEndPoint == sessionParams.ServerEndPoint &&
                    p.ChannelLifeTimeSec == sessionParams.ChannelLifeTimeSec &&
                    p.KeepAliveIntervalSec == sessionParams.KeepAliveIntervalSec &&
                    p.MaxBufferSizeBytes == sessionParams.MaxBufferSizeBytes &&
                    p.MaxMessageSizeBytes == sessionParams.MaxMessageSizeBytes &&
                    p.MinSubscriptionLifeTimeSec == sessionParams.MinSubscriptionLifeTimeSec &&
                    p.OperationTimeoutSec == sessionParams.OperationTimeoutSec &&
                    p.SecurityTokenLifeTimeSec == sessionParams.SecurityTokenLifeTimeSec &&
                    p.SessionTimeoutSec == sessionParams.SessionTimeoutSec
                ));

            var stored = _manager.GetSession("App1");
            Assert.Same(uaSessionMock, stored);
        }

        /// <summary>
        /// Ensures that if <see cref="OpcSessionsManager.AddSession"/> 
        /// is called twice for the same AppId, a warning is logged 
        /// and the existing session is not replaced silently.
        /// </summary>
        [Fact]
        public async Task AddSession_ShouldLogWarning_IfSessionAlreadyExists()
        {
            // Arrange
            var sessionParams = Substitute.For<ISessionChannelParams>();
            sessionParams.AppId.Returns("App2");
            sessionParams.ServerEndPoint.Returns("opc.tcp://localhost:4840");

            var sessionMock = Substitute.For<IOpcUaSession>();
            _factoryMock.GetSession(Arg.Any<ISessionParams>())
                        .Returns(Task.FromResult(sessionMock));

            await _manager.AddSession(sessionParams);

            // Act
            await _manager.AddSession(sessionParams);

            // Assert
            _loggerMock.Received().LogWarning("session for app App2 already exist");
        }

        /// <summary>
        /// Ensures that <see cref="OpcSessionsManager.GetSession"/> 
        /// returns null if no session with the given AppId exists.
        /// </summary>
        [Fact]
        public void GetSession_ShouldReturnNull_IfSessionDoesNotExist()
        {
            // Act
            var result = _manager.GetSession("Unknown");

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Ensures that <see cref="OpcSessionsManager.RemoveSession"/> 
        /// removes the stored session and disposes it.
        /// </summary>
        [Fact]
        public async Task RemoveSession_ShouldDisposeSession_IfExists()
        {
            // Arrange
            var sessionParams = Substitute.For<ISessionChannelParams>();
            sessionParams.AppId.Returns("App3");
            sessionParams.ServerEndPoint.Returns("opc.tcp://localhost:4840");

            var sessionMock = Substitute.For<IOpcUaSession>();
            _factoryMock.GetSession(Arg.Any<ISessionParams>())
                        .Returns(Task.FromResult(sessionMock));

            await _manager.AddSession(sessionParams);

            // Act
            _manager.RemoveSession("App3");

            // Assert
            sessionMock.Received(1).Dispose();
            Assert.Null(_manager.GetSession("App3"));
        }
    }
}
