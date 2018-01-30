using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxModel.Packets;
using NitroxModel.PlayerSlot;
using NSubstitute;
using System;

namespace NitroxTest.Client.Communication.ClientBridgeTests
{
    [TestClass]
    public class ConfirmReservationTests
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
        public void TheBridgeShouldBeReservedAfterConfirmingAReservation()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.confirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Reserved);
            this.clientBridge.ReservationRejectionReason.Should().Be(ReservationRejectionReason.None);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidReservationExceptionIfConfirmingAReservationWhileItIsNotWaiting()
        {
            //When
            Action action = () => this.clientBridge.confirmReservation(this.correlationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            action.ShouldThrow<InvalidReservationException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidReservationExceptionIfConfirmingAReserverationWhileItIsAlreadyReserved()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.confirmReservation(this.correlationId, TestConstants.TEST_RESERVATION_KEY);
            Action action = () => this.clientBridge.confirmReservation(null, null);

            //Then
            action.ShouldThrow<InvalidReservationException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidReservationExceptionIfConfirmingAReserverationWhileItIsAlreadyConnected()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.confirmReservation(this.correlationId, TestConstants.TEST_RESERVATION_KEY);
            this.clientBridge.claimReservation();
            Action action = () => this.clientBridge.confirmReservation(null, null);

            //Then
            action.ShouldThrow<InvalidReservationException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnUncorrelatedMessageExceptionIfConfirmingAReservationWithTheIncorrectCorrelationId()
        {
            //When
            var incorrectCorrelationId = "WRONG";
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.confirmReservation(incorrectCorrelationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            action.ShouldThrow<UncorrelatedMessageException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionIfConfirmaingAReservationWithANullCorrelationId()
        {
            //When
            string nullCorrelationId = null;
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.confirmReservation(nullCorrelationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "correlationId" && ex.Message == "The value cannot be null.");
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionIfConfirmaingAReservationWithABlankCorrelationId()
        {
            //When
            string blankCorrelationId = string.Empty;
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.confirmReservation(blankCorrelationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "correlationId" && ex.Message == "The value cannot be blank.");
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionIfConfirmaingAReservationWithANullReservationKey()
        {
            //When
            string nullReservationKey = null;
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.confirmReservation(this.correlationId, nullReservationKey);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "reservationKey" && ex.Message == "The value cannot be null.");
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);

        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionIfConfirmaingAReservationWithABlankReservationKey()
        {
            //When
            string blankReservationKey = string.Empty;
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.confirmReservation(this.correlationId, blankReservationKey);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "reservationKey" && ex.Message == "The value cannot be blank.");
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
