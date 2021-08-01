using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxModel.Packets;
using NSubstitute;

namespace NitroxTest.Client.Communication.MultiplayerSessionTests.ConnectionStateTests
{
    [TestClass]
    public class DisconnectedStateTests
    {
        [TestMethod]
        public void NegotiateShouldStartTheClientOnTheContext()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.Start(Arg.Any<string>(), TestConstants.TEST_SERVER_PORT))
                .Do(info => serverClient.IsConnected.Returns(true));

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);
            connectionContext.ServerPort.Returns(TestConstants.TEST_SERVER_PORT);

            Disconnected connectionState = new Disconnected();

            // Act
            connectionState.NegotiateReservation(connectionContext);

            // Assert
            serverClient.IsConnected.Should().BeTrue();
        }

        [TestMethod]
        public void NegotiateShouldSendMultiplayerSessionPolicyRequestPacketToClient()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.Start(Arg.Any<string>(), TestConstants.TEST_SERVER_PORT))
                .Do(info => serverClient.IsConnected.Returns(true));

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);
            connectionContext.ServerPort.Returns(TestConstants.TEST_SERVER_PORT);

            Disconnected connectionState = new Disconnected();

            // Act
            connectionState.NegotiateReservation(connectionContext);

            // Assert
            serverClient.Received().Send(Arg.Any<MultiplayerSessionPolicyRequest>());
        }

        [TestMethod]
        public void NegotiateShouldTransitionToEstablishingSessionPolicyState()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.Start(Arg.Any<string>(), TestConstants.TEST_SERVER_PORT))
                .Do(info => serverClient.IsConnected.Returns(true));

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);
            connectionContext.ServerPort.Returns(TestConstants.TEST_SERVER_PORT);

            Disconnected connectionState = new Disconnected();

            // Act
            connectionState.NegotiateReservation(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<EstablishingSessionPolicy>());
        }

        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationExceptionWhenClientIsNull()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns((IClient)null);
            connectionContext.IpAddress.Returns(TestConstants.TEST_IP_ADDRESS);

            Disconnected connectionState = new Disconnected();

            // Act
            Action action = () => connectionState.NegotiateReservation(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationExceptionWhenIpAddressIsNull()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(client);
            connectionContext.IpAddress.Returns((string)null);

            Disconnected connectionState = new Disconnected();

            // Act
            Action action = () => connectionState.NegotiateReservation(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void JoinSessionShouldThrowInvalidOperationException()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            Disconnected connectionState = new Disconnected();

            // Act
            Action action = () => connectionState.JoinSession(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void DisconnectShouldThrowInvalidOperationException()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            Disconnected connectionState = new Disconnected();

            // Act
            Action action = () => connectionState.Disconnect(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }
    }
}
