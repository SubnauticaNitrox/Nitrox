using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NSubstitute;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using FluentAssertions;
using NitroxClient.Communication.Exceptions;
using NitroxModel.PlayerSlot;

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
            var serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.Start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.Send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => this.correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            this.clientBridge = new ClientBridge(serverClient);
        }

        [TestMethod]
        public void ClaimingAReservationOnAReservedBridgeShouldMakeItConnected()
        {
            //When
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            this.clientBridge.ClaimReservation();

            //Then
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Connected);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnADisconnectedBridge()
        {
            //When
            Action action = () => this.clientBridge.ClaimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnAWaitingBrige()
        {
            //When
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.ClaimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingARejectedReservation()
        {
            //When
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.HandleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);
            Action action = () => this.clientBridge.ClaimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnAConnectedBridge()
        {
            //When
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            this.clientBridge.ClaimReservation();
            Action action = () => this.clientBridge.ClaimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
