using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test.Client.Communication.MultiplayerSession;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NSubstitute;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
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
                .When(client => client.StartAsync(Arg.Any<string>(), TestConstants.TEST_SERVER_PORT))
                .Do(info => serverClient.IsConnected.Returns(true));

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);
            connectionContext.ServerPort.Returns(TestConstants.TEST_SERVER_PORT);

            Disconnected connectionState = new Disconnected();

            // Act
            connectionState.NegotiateReservationAsync(connectionContext);

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
                .When(client => client.StartAsync(Arg.Any<string>(), TestConstants.TEST_SERVER_PORT))
                .Do(info => serverClient.IsConnected.Returns(true));

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);
            connectionContext.ServerPort.Returns(TestConstants.TEST_SERVER_PORT);

            Disconnected connectionState = new Disconnected();

            // Act
            connectionState.NegotiateReservationAsync(connectionContext);

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
                .When(client => client.StartAsync(Arg.Any<string>(), TestConstants.TEST_SERVER_PORT))
                .Do(info => serverClient.IsConnected.Returns(true));

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);
            connectionContext.ServerPort.Returns(TestConstants.TEST_SERVER_PORT);

            Disconnected connectionState = new Disconnected();

            // Act
            connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<EstablishingSessionPolicy>());
        }

        [TestMethod]
        public async Task NegotiateShouldThrowInvalidOperationExceptionWhenClientIsNull()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns((IClient)null);
            connectionContext.IpAddress.Returns(TestConstants.TEST_IP_ADDRESS);

            Disconnected connectionState = new Disconnected();

            // Act
            Func<Task> action = async () => await connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [TestMethod]
        public async Task NegotiateShouldThrowInvalidOperationExceptionWhenIpAddressIsNull()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(client);
            connectionContext.IpAddress.Returns((string)null);

            Disconnected connectionState = new Disconnected();

            // Act
            Func<Task> action = async () => await connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>();
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
