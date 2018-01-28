using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NitroxClient.Communication.Abstract;
using FluentAssertions;
using NitroxClient.Communication;
using NitroxClient.Communication.Exceptions;
using NitroxModel.Packets;

namespace NitroxTest.Client.Communication.ClientBridgeTests
{
    [TestClass]
    public class ConnectRetryTests
    {
        private ClientBridge clientBridge;
        private string correlationId;

        [TestInitialize]
        public void GivenAClientBridgeThatIsWaitingForAReservation()
        {
            //Given
            var serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            clientBridge = new ClientBridge(serverClient);
            clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
        }

        [TestMethod]
        public void ShouldBeAbleToCallConnectAgainAfterHandlingAReservationRejection()
        {
            //When            
            clientBridge.handleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);
            clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Then
            clientBridge.CurrentState.Should().Be(ClientBridgeState.WaitingForRerservation);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedConnectAttemptExceptionWhenConnectIsCalledOnABridgeThatIsAlreadyWaiting()
        {
            //When
            Action action = () => clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Then
            action.ShouldThrow<ProhibitedConnectAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedConnectAttemptExceptionWhenConnectIsCalledOnAReservedBridge()
        {
            //When
            clientBridge.confirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            Action action = () => clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Then
            action.ShouldThrow<ProhibitedConnectAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedConnectAttemptExceptionWhenConnectIsCalledOnAConnectedBridge()
        {
            //When
            clientBridge.confirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            clientBridge.claimReservation();
            Action action = () => clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Then
            action.ShouldThrow<ProhibitedConnectAttemptException>();
        }
    }
}
