using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication.Abstract;
using NSubstitute;
using FluentAssertions;
using NitroxClient.Communication;
using NitroxClient.Communication.Exceptions;

namespace NitroxTest.Client.Communication.ClientBridgeTests
{
    [TestClass]
    public class ConnectTests
    {
        [TestMethod]
        public void TheBridgeShouldBeInTheWaitingStateAfterConnecting()
        {
            //Arrange
            var serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            var clientBridge = new ClientBridge(serverClient);

            //Act
            clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Assert
            clientBridge.CurrentState.Should().Be(ClientBridgeState.WaitingForRerservation);
        }

        [TestMethod]
        public void ShouldNotStartAConnectedClient()
        {
            //Arrange
            var serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(true);

            var clientBridge = new ClientBridge(serverClient);

            //Act
            clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Assert
            serverClient.DidNotReceive().start(Arg.Any<string>());
            clientBridge.CurrentState.Should().Be(ClientBridgeState.WaitingForRerservation);
        }

        [TestMethod]
        public void ConnectShouldThrowClientConnectionFailedExceptionAndBeInFaultedStateOnFailure()
        {
            //Arrange
            var serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.start(Arg.Any<string>()))
                .Throw<Exception>();

            var clientBridge = new ClientBridge(serverClient);

            //Act
            Action action = () => clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Assert
            action.ShouldThrow<ClientConnectionFailedException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
