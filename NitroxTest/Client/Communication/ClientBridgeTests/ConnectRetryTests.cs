using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NitroxClient.Communication.Abstract;
using FluentAssertions;
using NitroxClient.Communication;
using NitroxClient.Communication.Exceptions;
using NitroxModel.Packets;
using NitroxModel.PlayerSlot;

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
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.Start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.Send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            clientBridge = new ClientBridge(serverClient);
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
        }

        [TestMethod]
        public void ShouldBeAbleToCallConnectAgainAfterHandlingAReservationRejection()
        {
            //When            
            clientBridge.HandleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Then
            clientBridge.CurrentState.Should().Be(ClientBridgeState.WaitingForRerservation);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedConnectAttemptExceptionWhenConnectIsCalledOnABridgeThatIsAlreadyWaiting()
        {
            //When
            Action action = () => clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Then
            action.ShouldThrow<ProhibitedConnectAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedConnectAttemptExceptionWhenConnectIsCalledOnAReservedBridge()
        {
            //When
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            Action action = () => clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Then
            action.ShouldThrow<ProhibitedConnectAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedConnectAttemptExceptionWhenConnectIsCalledOnAConnectedBridge()
        {
            //When
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            clientBridge.ClaimReservation();
            Action action = () => clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);

            //Then
            action.ShouldThrow<ProhibitedConnectAttemptException>();
        }
    }
}
