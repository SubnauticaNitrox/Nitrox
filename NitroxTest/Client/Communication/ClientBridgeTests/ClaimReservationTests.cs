using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NSubstitute;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using FluentAssertions;
using NitroxClient.Communication.Exceptions;

namespace NitroxTest.Client.Communication.ClientBridgeTests
{
    [TestClass]
    public class ClaimReservationTests
    {
        private ClientBridge clientBridge;
        private string correlationId;

        [TestInitialize]
        public void GivenAnInitializedClientBridge()
        {
            //Given
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.Start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.Send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => this.correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            clientBridge = new ClientBridge(serverClient);
        }

        [TestMethod]
        public void ClaimingAReservationOnAReservedBridgeShouldMakeItConnected()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            clientBridge.ClaimReservation();

            //Then
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Connected);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnADisconnectedBridge()
        {
            //When
            Action action = () => this.clientBridge.ClaimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnAWaitingBrige()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.ClaimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingARejectedReservation()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.HandleRejectedReservation(correlationId, TestConstants.TEST_REJECTION_STATE);
            Action action = () => this.clientBridge.ClaimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnAConnectedBridge()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            clientBridge.ClaimReservation();
            Action action = () => this.clientBridge.ClaimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
