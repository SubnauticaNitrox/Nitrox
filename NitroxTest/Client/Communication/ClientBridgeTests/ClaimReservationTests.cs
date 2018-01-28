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
            var serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => this.correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            this.clientBridge = new ClientBridge(serverClient);
        }

        [TestMethod]
        public void ClaimingAReservationOnAReservedBridgeShouldMakeItConnected()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.confirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            this.clientBridge.claimReservation();

            //Then
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Connected);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnADisconnectedBridge()
        {
            //When
            Action action = () => this.clientBridge.claimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnAWaitingBrige()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.claimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingARejectedReservation()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.handleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);
            Action action = () => this.clientBridge.claimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void ShouldThrowAProhibitedClaimAttemptExceptionWhenClaimingAReservationOnAConnectedBridge()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.confirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            this.clientBridge.claimReservation();
            Action action = () => this.clientBridge.claimReservation();

            //Then
            action.ShouldThrow<ProhibitedClaimAttemptException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
